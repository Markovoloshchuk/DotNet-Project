using System.Diagnostics;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace Platform.ServiceDefaults.Observability;

public sealed class GrpcClientTracingInterceptor : Interceptor
{
    private readonly ILogger<GrpcClientTracingInterceptor> _logger;

    public GrpcClientTracingInterceptor(
        ILogger<GrpcClientTracingInterceptor> logger)
    {
        _logger = logger;
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<
        TRequest,
        TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var fullMethodName = context.Method.FullName;

        // Host can be null, so provide a fallback value.
        var host = context.Host ?? "unknown";

        var stopwatch = Stopwatch.StartNew();

        var activity = GrpcTelemetry.StartActivity(
            fullMethodName,
            ActivityKind.Client,
            host);

        AsyncUnaryCall<TResponse> call;

        try
        {
            call = continuation(request, context);
        }
        catch (RpcException exception)
        {
            CompleteCall(
                activity,
                stopwatch,
                fullMethodName,
                host,
                exception.StatusCode,
                exception.Status.Detail);

            throw;
        }
        catch (OperationCanceledException exception)
        {
            CompleteCall(
                activity,
                stopwatch,
                fullMethodName,
                host,
                StatusCode.Cancelled,
                exception.Message);

            throw;
        }
        catch (Exception exception)
        {
            CompleteCall(
                activity,
                stopwatch,
                fullMethodName,
                host,
                StatusCode.Unknown,
                exception.Message);

            throw;
        }

        var responseTask = ObserveResponseAsync(
            call.ResponseAsync,
            activity,
            stopwatch,
            fullMethodName,
            host);

        return new AsyncUnaryCall<TResponse>(
            responseTask,
            call.ResponseHeadersAsync,
            call.GetStatus,
            call.GetTrailers,
            call.Dispose);
    }

    private async Task<TResponse> ObserveResponseAsync<TResponse>(
        Task<TResponse> responseTask,
        Activity? activity,
        Stopwatch stopwatch,
        string fullMethodName,
        string host)
    {
        var statusCode = StatusCode.OK;
        string? statusDescription = null;

        try
        {
            return await responseTask.ConfigureAwait(false);
        }
        catch (RpcException exception)
        {
            statusCode = exception.StatusCode;
            statusDescription = exception.Status.Detail;

            _logger.LogWarning(
                exception,
                "gRPC client call failed: {GrpcMethod}, " +
                "Status: {StatusCode}",
                fullMethodName,
                statusCode);

            throw;
        }
        catch (OperationCanceledException exception)
        {
            statusCode = StatusCode.Cancelled;
            statusDescription = exception.Message;

            throw;
        }
        catch (Exception exception)
        {
            statusCode = StatusCode.Unknown;
            statusDescription = exception.Message;

            _logger.LogError(
                exception,
                "Unexpected gRPC client error: {GrpcMethod}",
                fullMethodName);

            throw;
        }
        finally
        {
            CompleteCall(
                activity,
                stopwatch,
                fullMethodName,
                host,
                statusCode,
                statusDescription);
        }
    }

    private void CompleteCall(
        Activity? activity,
        Stopwatch stopwatch,
        string fullMethodName,
        string host,
        StatusCode statusCode,
        string? statusDescription)
    {
        stopwatch.Stop();

        GrpcTelemetry.SetStatus(
            activity,
            statusCode,
            statusDescription);

        _logger.LogInformation(
            "gRPC client call completed. " +
            "Method: {GrpcMethod}, Host: {Host}, " +
            "Status: {StatusCode}, DurationMs: {DurationMs}",
            fullMethodName,
            host,
            statusCode,
            stopwatch.Elapsed.TotalMilliseconds);

        activity?.Dispose();
    }
}