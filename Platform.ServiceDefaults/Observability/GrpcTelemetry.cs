using System.Diagnostics;
using Grpc.Core;

namespace Platform.ServiceDefaults.Observability;

public static class GrpcTelemetry
{
    public static readonly ActivitySource ActivitySource =
        new(TelemetryNames.GrpcActivitySource, "1.0.0");

    public static Activity? StartActivity(
        string fullMethodName,
        ActivityKind activityKind,
        string? peerAddress = null)
    {
        var (serviceName, methodName) =
            ParseMethodName(fullMethodName);

        var activity = ActivitySource.StartActivity(
            $"{serviceName}/{methodName}",
            activityKind);

        activity?.SetTag("rpc.system", "grpc");
        activity?.SetTag("rpc.service", serviceName);
        activity?.SetTag("rpc.method", methodName);

        if (!string.IsNullOrWhiteSpace(peerAddress))
        {
            var addressTag = activityKind == ActivityKind.Client
                ? "server.address"
                : "network.peer.address";

            activity?.SetTag(addressTag, peerAddress);
        }

        return activity;
    }

    public static void SetStatus(
        Activity? activity,
        StatusCode statusCode,
        string? description = null)
    {
        activity?.SetTag(
            "rpc.grpc.status_code",
            (int)statusCode);

        if (statusCode == StatusCode.OK)
        {
            activity?.SetStatus(ActivityStatusCode.Ok);
            return;
        }

        activity?.SetStatus(
            ActivityStatusCode.Error,
            description ?? statusCode.ToString());
    }

    private static (string Service, string Method) ParseMethodName(
        string fullMethodName)
    {
        var normalizedName = fullMethodName.Trim('/');

        var separatorIndex = normalizedName.LastIndexOf('/');

        if (separatorIndex <= 0
            || separatorIndex == normalizedName.Length - 1)
        {
            return ("unknown", normalizedName);
        }

        var serviceName = normalizedName[..separatorIndex];
        var methodName = normalizedName[(separatorIndex + 1)..];

        return (serviceName, methodName);
    }
}