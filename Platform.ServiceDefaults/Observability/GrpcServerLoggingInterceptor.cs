using System.Diagnostics;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace Platform.ServiceDefaults.Observability;

public sealed class GrpcServerLoggingInterceptor : Interceptor
{
    private readonly ILogger<GrpcServerLoggingInterceptor> _logger;

    public GrpcServerLoggingInterceptor(
        ILogger<GrpcServerLoggingInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<
        TRequest,
        TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var stopwatch = Stopwatch.StartNew();

        var activity = GrpcTelemetry.StartActivity(
            context.Method,
            ActivityKind.Server,
            context.Peer);

        var statusCode = StatusCode.OK;
        string? statusDescription = null;

        try
        {
            return await continuation(
                request,
                context);
        }
        catch (RpcException exception)
        {
            statusCode = exception.StatusCode;
            statusDescription = exception.Status.Detail;

            _logger.LogWarning(
                exception,
                "gRPC server call failed: {GrpcMethod}, " +
                "Status: {StatusCode}",
                context.Method,
                statusCode);

            throw;
        }
        catch (OperationCanceledException exception)
        {
            statusCode = StatusCode.Cancelled;
            statusDescription = exception.Message;

            _logger.LogWarning(
                exception,
                "gRPC server call was cancelled: {GrpcMethod}",
                context.Method);

            throw;
        }
        catch (Exception exception)
        {
            statusCode = StatusCode.Unknown;
            statusDescription = exception.Message;

            _logger.LogError(
                exception,
                "Unexpected error during gRPC server call: {GrpcMethod}",
                context.Method);

            throw;
        }
        finally
        {
            stopwatch.Stop();

            GrpcTelemetry.SetStatus(
                activity,
                statusCode,
                statusDescription);

            _logger.LogInformation(
                "gRPC server call completed. " +
                "Method: {GrpcMethod}, Peer: {Peer}, " +
                "Status: {StatusCode}, DurationMs: {DurationMs}",
                context.Method,
                context.Peer,
                statusCode,
                stopwatch.Elapsed.TotalMilliseconds);

            activity?.Dispose();
        }
    }
}