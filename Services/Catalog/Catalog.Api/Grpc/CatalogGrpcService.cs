
using System.Globalization;
using Catalog.Api.Grpc.Contracts;
using Catalog.Bll.DTOs.MedicalServices;
using Catalog.Bll.Interfaces;
using Catalog.Domain.Exceptions;
using Grpc.Core;

namespace Catalog.Api.Grpc;

public sealed class CatalogGrpcService(
    IMedicalServiceService medicalServiceService)
    : CatalogGrpc.CatalogGrpcBase
{
    public override async Task<MedicalServiceResponse> GetProduct(
        GetProductRequest request,
        ServerCallContext context)
    {
        if (request.Id <= 0)
        {
            throw InvalidArgument(
                "Medical service ID must be greater than zero.");
        }

        try
        {
            var service = await medicalServiceService.GetByIdAsync(
                request.Id,
                context.CancellationToken);

            return MapToResponse(service);
        }
        catch (NotFoundException exception)
        {
            throw NotFound(exception);
        }
        catch (DomainException exception)
        {
            throw InvalidArgument(exception.Message);
        }
        catch (ArgumentException exception)
        {
            throw InvalidArgument(exception.Message);
        }
    }

    public override async Task<GetProductsByIdsResponse>
        GetProductsByIds(
            GetProductsByIdsRequest request,
            ServerCallContext context)
    {
        if (request.Ids.Count is < 1 or > 100)
        {
            throw InvalidArgument(
                "The request must contain between 1 and 100 IDs.");
        }

        if (request.Ids.Any(id => id <= 0))
        {
            throw InvalidArgument(
                "All medical service IDs must be greater than zero.");
        }

        var response = new GetProductsByIdsResponse();

        try
        {
            // Sequential calls reuse the BLL cache.
            // This also avoids concurrent EF Core operations
            // on the same scoped DbContext.
            foreach (var id in request.Ids.Distinct())
            {
                var service = await medicalServiceService.GetByIdAsync(
                    id,
                    context.CancellationToken);

                response.Products.Add(
                    MapToResponse(service));
            }

            return response;
        }
        catch (NotFoundException exception)
        {
            throw NotFound(exception);
        }
        catch (DomainException exception)
        {
            throw InvalidArgument(exception.Message);
        }
        catch (ArgumentException exception)
        {
            throw InvalidArgument(exception.Message);
        }
    }

    private static MedicalServiceResponse MapToResponse(
        MedicalServiceDto service)
    {
        var response = new MedicalServiceResponse
        {
            ServiceId = service.ServiceId,
            CategoryId = service.CategoryId,
            CategoryName = service.CategoryName,
            EquipmentId = service.EquipmentId ?? 0,
            EquipmentName = service.EquipmentName ?? string.Empty,
            ServiceName = service.ServiceName,

            Price = service.Price.ToString(
                CultureInfo.InvariantCulture),

            DurationMinutes = service.DurationMinutes,
            IsActive = service.IsActive
        };

        if (service.Detail is not null)
        {
            response.Detail = new ServiceDetailResponse
            {
                ServiceDetailId =
                    service.Detail.ServiceDetailId,

                PreparationInstructions =
                    service.Detail.PreparationInstructions
                    ?? string.Empty,

                Contraindications =
                    service.Detail.Contraindications
                    ?? string.Empty
            };
        }

        response.Packages.AddRange(
            service.Packages.Select(package =>
                new MedicalServicePackageResponse
                {
                    PackageId = package.PackageId,
                    PackageName = package.PackageName,
                    Quantity = package.Quantity,
                    CustomNotes = package.CustomNotes ?? string.Empty
                }));

        return response;
    }

    private static RpcException NotFound(
        NotFoundException exception)
    {
        return new RpcException(
            new Status(
                StatusCode.NotFound,
                exception.Message));
    }

    private static RpcException InvalidArgument(
        string message)
    {
        return new RpcException(
            new Status(
                StatusCode.InvalidArgument,
                message));
    }
}
