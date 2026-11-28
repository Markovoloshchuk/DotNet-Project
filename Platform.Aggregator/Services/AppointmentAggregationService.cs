using System.Globalization;
using System.Text.Json;
using Appointments.Api.Grpc.Contracts;
using Catalog.Api.Grpc.Contracts;
using Grpc.Core;
using Microsoft.Extensions.Caching.Distributed;
using Platform.Aggregator.Clients;
using Platform.Aggregator.Models;

namespace Platform.Aggregator.Services;

public sealed class AppointmentAggregationService
{
    private const int CatalogBatchSize = 100;

    private static readonly TimeSpan GrpcTimeout =
        TimeSpan.FromSeconds(5);

    private static readonly TimeSpan CacheLifetime =
        TimeSpan.FromSeconds(45);

    private const string CacheKeyPrefix =
        "aggregator:appointment-full:v1:";

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly AppointmentsGrpc.AppointmentsGrpcClient
        _appointmentsClient;

    private readonly CatalogGrpc.CatalogGrpcClient
        _catalogClient;

    private readonly FeedbackClient _feedbackClient;

    private readonly IDistributedCache _distributedCache;

    private readonly ILogger<AppointmentAggregationService> _logger;

    public AppointmentAggregationService(
        AppointmentsGrpc.AppointmentsGrpcClient appointmentsClient,
        CatalogGrpc.CatalogGrpcClient catalogClient,
        FeedbackClient feedbackClient,
        IDistributedCache distributedCache,
        ILogger<AppointmentAggregationService> logger)
    {
        _appointmentsClient = appointmentsClient;
        _catalogClient = catalogClient;
        _feedbackClient = feedbackClient;
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task<AppointmentFullResponse> GetFullAsync(
        int appointmentId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting aggregation for appointment {AppointmentId}",
            appointmentId);

        // Step 1: Check the distributed cache.
        var cachedResponse = await TryGetCachedAsync(
            appointmentId,
            cancellationToken);

        if (cachedResponse is not null)
        {
            _logger.LogInformation(
                "Aggregation cache hit for appointment {AppointmentId}",
                appointmentId);

            return cachedResponse;
        }

        _logger.LogDebug(
            "Aggregation cache miss for appointment {AppointmentId}",
            appointmentId);

        // Step 2: Retrieve the required appointment through gRPC.
        var appointment = await _appointmentsClient.GetAppointmentByIdAsync(
            new GetAppointmentByIdRequest
            {
                Id = appointmentId
            },
            deadline: DateTime.UtcNow.Add(GrpcTimeout),
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Appointment {AppointmentId} retrieved through gRPC",
            appointmentId);

        // Step 3: Retrieve Catalog and Feedback concurrently.
        var catalogTask = TryGetOptionalAsync(
            "Catalog",
            ct => GetCatalogAsync(appointment, ct),
            cancellationToken);

        var feedbackTask = TryGetOptionalAsync(
            "Feedback",
            ct => _feedbackClient.GetForAppointmentAsync(
                appointment.PatientId,
                appointment.DoctorId,
                ct),
            cancellationToken);

        var optionalResults = await Task.WhenAll(
            catalogTask,
            feedbackTask);

        var catalogResult = optionalResults[0];
        var feedbackResult = optionalResults[1];

        var warnings = new List<string>();

        if (catalogResult.Warning is not null)
        {
            warnings.Add(catalogResult.Warning);
        }

        if (feedbackResult.Warning is not null)
        {
            warnings.Add(feedbackResult.Warning);
        }

        // Step 4: Map the gRPC response to the existing HTTP response model.
        var result = new AppointmentFullResponse(
            Appointment: MapAppointment(appointment),
            Catalog: catalogResult.Data,
            Feedback: feedbackResult.Data,
            Warnings: warnings);

        // Step 5: Cache only complete aggregation results.
        if (warnings.Count == 0)
        {
            await TrySetCachedAsync(
                appointmentId,
                result,
                cancellationToken);
        }
        else
        {
            _logger.LogInformation(
                "Partial aggregation for appointment {AppointmentId} " +
                "will not be cached",
                appointmentId);
        }

        _logger.LogInformation(
            "Aggregation completed for appointment {AppointmentId}. " +
            "Warnings: {WarningCount}",
            appointmentId,
            warnings.Count);

        return result;
    }

    private async Task<JsonElement> GetCatalogAsync(
        AppointmentResponse appointment,
        CancellationToken cancellationToken)
    {
        var serviceIds = appointment.Services
            .Select(service => service.ServiceId)
            .Where(id => id > 0)
            .Distinct()
            .ToArray();

        // An appointment without medical services does not need a gRPC call.
        if (serviceIds.Length == 0)
        {
            return CreateCatalogJson(
                Array.Empty<MedicalServiceResponse>());
        }

        // The Catalog API accepts up to 100 IDs per request.
        // Split larger collections into batches.
        var batchTasks = serviceIds
            .Chunk(CatalogBatchSize)
            .Select(async batch =>
            {
                var request = new GetProductsByIdsRequest();
                request.Ids.AddRange(batch);

                var response = await _catalogClient.GetProductsByIdsAsync(
                    request,
                    deadline: DateTime.UtcNow.Add(GrpcTimeout),
                    cancellationToken: cancellationToken);

                return response.Products.ToArray();
            })
            .ToArray();

        // Each batch is an independent gRPC call.
        var batchResults = await Task.WhenAll(batchTasks);

        var products = batchResults
            .SelectMany(batch => batch)
            .ToArray();

        _logger.LogInformation(
            "Retrieved {ProductCount} Catalog products through gRPC",
            products.Length);

        return CreateCatalogJson(products);
    }

    private static JsonElement MapAppointment(
        AppointmentResponse appointment)
    {
        var result = new
        {
            appointmentId = appointment.AppointmentId,
            patientId = appointment.PatientId,
            doctorId = appointment.DoctorId,
            appointmentDate = appointment.AppointmentDate.ToDateTime(),
            status = appointment.Status,
            totalAmount = ParseDecimal(appointment.TotalAmount),
            createdAt = appointment.CreatedAt.ToDateTime(),

            services = appointment.Services
                .Select(service => new
                {
                    appointmentServiceId =
                        service.AppointmentServiceId,

                    serviceId = service.ServiceId,

                    serviceNameSnapshot =
                        service.ServiceNameSnapshot,

                    priceSnapshot =
                        ParseDecimal(service.PriceSnapshot),

                    quantity = service.Quantity
                })
                .ToArray()
        };

        return JsonSerializer.SerializeToElement(
            result,
            JsonOptions);
    }

    private static JsonElement CreateCatalogJson(
        IReadOnlyList<MedicalServiceResponse> products)
    {
        var result = new
        {
            items = products
                .Select(MapCatalogProduct)
                .ToArray(),

            pageNumber = 1,
            pageSize = products.Count,
            totalCount = products.Count,
            totalPages = products.Count == 0 ? 0 : 1,
            hasPreviousPage = false,
            hasNextPage = false
        };

        return JsonSerializer.SerializeToElement(
            result,
            JsonOptions);
    }

    private static object MapCatalogProduct(
        MedicalServiceResponse product)
    {
        return new
        {
            serviceId = product.ServiceId,
            categoryId = product.CategoryId,
            categoryName = product.CategoryName,

            equipmentId = product.EquipmentId == 0
                ? (long?)null
                : product.EquipmentId,

            equipmentName = EmptyToNull(product.EquipmentName),

            serviceName = product.ServiceName,
            price = ParseDecimal(product.Price),
            durationMinutes = product.DurationMinutes,
            isActive = product.IsActive,

            detail = product.Detail is null
                ? null
                : new
                {
                    serviceDetailId =
                        product.Detail.ServiceDetailId,

                    preparationInstructions =
                        EmptyToNull(
                            product.Detail.PreparationInstructions),

                    contraindications =
                        EmptyToNull(
                            product.Detail.Contraindications)
                },

            packages = product.Packages
                .Select(package => new
                {
                    packageId = package.PackageId,
                    packageName = package.PackageName,
                    quantity = package.Quantity,
                    customNotes = EmptyToNull(package.CustomNotes)
                })
                .ToArray()
        };
    }

    private async Task<OptionalServiceResult> TryGetOptionalAsync(
        string serviceName,
        Func<CancellationToken, Task<JsonElement>> request,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await request(cancellationToken);

            _logger.LogInformation(
                "{ServiceName} data retrieved successfully",
                serviceName);

            return new OptionalServiceResult(data, null);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
            when (exception is not OutOfMemoryException
                  && exception is not StackOverflowException)
        {
            var reason = exception switch
            {
                RpcException rpcException
                    when rpcException.StatusCode == StatusCode.Unavailable
                    => "gRPC Unavailable",

                RpcException rpcException
                    when rpcException.StatusCode == StatusCode.DeadlineExceeded
                    => "gRPC DeadlineExceeded",

                RpcException rpcException
                    => $"gRPC {rpcException.StatusCode}",

                HttpRequestException httpException
                    when httpException.StatusCode.HasValue
                    => $"HTTP {(int)httpException.StatusCode.Value}",

                OperationCanceledException
                    => "request timeout",

                JsonException
                    => "invalid JSON response",

                _ => "request failed"
            };

            var warning =
                $"{serviceName} service is unavailable ({reason}).";

            _logger.LogWarning(
                exception,
                "{ServiceName} request failed. Returning partial response.",
                serviceName);

            return new OptionalServiceResult(null, warning);
        }
    }

    private async Task<AppointmentFullResponse?> TryGetCachedAsync(
        int appointmentId,
        CancellationToken cancellationToken)
    {
        var cacheKey = GetCacheKey(appointmentId);

        try
        {
            var cachedJson = await _distributedCache.GetStringAsync(
                cacheKey,
                cancellationToken);

            if (string.IsNullOrWhiteSpace(cachedJson))
            {
                return null;
            }

            var cachedResponse =
                JsonSerializer.Deserialize<AppointmentFullResponse>(
                    cachedJson,
                    JsonOptions);

            if (cachedResponse is null)
            {
                return null;
            }

            _logger.LogDebug(
                "Loaded appointment {AppointmentId} aggregation from Redis",
                appointmentId);

            return cachedResponse;
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
            when (exception is not OutOfMemoryException
                  && exception is not StackOverflowException)
        {
            _logger.LogWarning(
                exception,
                "Redis read failed for appointment {AppointmentId}. " +
                "Continuing without aggregation cache.",
                appointmentId);

            return null;
        }
    }

    private async Task TrySetCachedAsync(
        int appointmentId,
        AppointmentFullResponse response,
        CancellationToken cancellationToken)
    {
        var cacheKey = GetCacheKey(appointmentId);

        try
        {
            var json = JsonSerializer.Serialize(
                response,
                JsonOptions);

            await _distributedCache.SetStringAsync(
                cacheKey,
                json,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = CacheLifetime
                },
                cancellationToken);

            _logger.LogDebug(
                "Cached complete aggregation for appointment {AppointmentId}",
                appointmentId);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
            when (exception is not OutOfMemoryException
                  && exception is not StackOverflowException)
        {
            _logger.LogWarning(
                exception,
                "Redis write failed for appointment {AppointmentId}.",
                appointmentId);
        }
    }

    private static string GetCacheKey(int appointmentId)
    {
        return $"{CacheKeyPrefix}{appointmentId}";
    }

    private static decimal ParseDecimal(string value)
    {
        return decimal.Parse(
            value,
            NumberStyles.Number,
            CultureInfo.InvariantCulture);
    }

    private static string? EmptyToNull(string value)
    {
        return string.IsNullOrEmpty(value)
            ? null
            : value;
    }

    private sealed record OptionalServiceResult(
        JsonElement? Data,
        string? Warning);
}