using Microsoft.AspNetCore.Http;

namespace Platform.ServiceDefaults;

public sealed class CorrelationIdDelegatingHandler
    : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdDelegatingHandler(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var correlationId = _httpContextAccessor
            .HttpContext?
            .Request.Headers[CorrelationIdMiddleware.HeaderName]
            .ToString();

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            request.Headers.Remove(
                CorrelationIdMiddleware.HeaderName);

            request.Headers.TryAddWithoutValidation(
                CorrelationIdMiddleware.HeaderName,
                correlationId);
        }

        return base.SendAsync(
            request,
            cancellationToken);
    }
}