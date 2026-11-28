using System.Diagnostics.Metrics;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Catalog.Bll.Caching;

public sealed class CatalogCacheService : ICatalogCacheService
{
    private const string CacheMeterName = "Clinic.Catalog.Cache";

    private static readonly TimeSpan L1SlidingExpiration =
        TimeSpan.FromSeconds(15);

    private static readonly TimeSpan L1AbsoluteExpiration =
        TimeSpan.FromSeconds(30);

    private static readonly TimeSpan L2AbsoluteExpiration =
        TimeSpan.FromMinutes(5);

    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<CatalogCacheService> _logger;

    private readonly Counter<long> _cacheHits;
    private readonly Counter<long> _cacheMisses;
    private readonly Counter<long> _cacheInvalidations;

    public CatalogCacheService(
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        ILogger<CatalogCacheService> logger,
        IMeterFactory meterFactory)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _logger = logger;

        var meter = meterFactory.Create(CacheMeterName);

        _cacheHits = meter.CreateCounter<long>(
            "clinic.cache.hits",
            description: "Number of Catalog cache hits.");

        _cacheMisses = meter.CreateCounter<long>(
            "clinic.cache.misses",
            description: "Number of Catalog cache misses.");

        _cacheInvalidations = meter.CreateCounter<long>(
            "clinic.cache.invalidations",
            description: "Number of Catalog cache invalidations.");
    }

    public async Task<T?> GetAsync<T>(
        string key,
        CancellationToken ct = default)
        where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        // L1: local in-memory cache.
        if (_memoryCache.TryGetValue(key, out var memoryValue)
            && memoryValue is T cachedValue)
        {
            RecordHit("L1");

            _logger.LogDebug(
                "L1 cache HIT for key {CacheKey}",
                key);

            return cachedValue;
        }

        RecordMiss("L1");

        _logger.LogDebug(
            "L1 cache MISS for key {CacheKey}",
            key);

        // L2: distributed Redis cache.
        try
        {
            var cachedBytes = await _distributedCache.GetAsync(
                key,
                ct);

            if (cachedBytes is null || cachedBytes.Length == 0)
            {
                RecordMiss("L2");

                _logger.LogDebug(
                    "L2 cache MISS for key {CacheKey}",
                    key);

                return null;
            }

            var distributedValue =
                JsonSerializer.Deserialize<T>(cachedBytes);

            if (distributedValue is null)
            {
                RecordMiss("L2");

                _logger.LogWarning(
                    "L2 cache returned an empty value for key {CacheKey}",
                    key);

                return null;
            }

            RecordHit("L2");

            _logger.LogDebug(
                "L2 cache HIT for key {CacheKey}",
                key);

            SetMemoryCache(key, distributedValue);

            return distributedValue;
        }
        catch (OperationCanceledException)
            when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            RecordMiss("L2");

            _logger.LogWarning(
                exception,
                "L2 cache read failed for key {CacheKey}. " +
                "Falling back to database.",
                key);

            return null;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        CancellationToken ct = default)
        where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        SetMemoryCache(key, value);

        try
        {
            var serializedValue =
                JsonSerializer.SerializeToUtf8Bytes(value);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    L2AbsoluteExpiration
            };

            await _distributedCache.SetAsync(
                key,
                serializedValue,
                options,
                ct);

            _logger.LogDebug(
                "Value stored in L1 and L2 for key {CacheKey}",
                key);
        }
        catch (OperationCanceledException)
            when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "L2 cache write failed for key {CacheKey}. " +
                "Value remains available in L1.",
                key);
        }
    }

    public async Task RemoveAsync(
        string key,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        _memoryCache.Remove(key);

        RecordInvalidation("L1");

        _logger.LogInformation(
            "L1 cache entry invalidated for key {CacheKey}",
            key);

        try
        {
            await _distributedCache.RemoveAsync(
                key,
                ct);

            RecordInvalidation("L2");

            _logger.LogInformation(
                "L2 cache entry invalidated for key {CacheKey}",
                key);
        }
        catch (OperationCanceledException)
            when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "L2 cache invalidation failed for key {CacheKey}.",
                key);
        }
    }

    private void SetMemoryCache<T>(
        string key,
        T value)
        where T : class
    {
        var options = new MemoryCacheEntryOptions()
            .SetSize(1)
            .SetSlidingExpiration(L1SlidingExpiration)
            .SetAbsoluteExpiration(L1AbsoluteExpiration);

        _memoryCache.Set(key, value, options);
    }

    private void RecordHit(string level)
    {
        _cacheHits.Add(
            1,
            new KeyValuePair<string, object?>("cache.level", level));
    }

    private void RecordMiss(string level)
    {
        _cacheMisses.Add(
            1,
            new KeyValuePair<string, object?>("cache.level", level));
    }

    private void RecordInvalidation(string level)
    {
        _cacheInvalidations.Add(
            1,
            new KeyValuePair<string, object?>("cache.level", level));
    }
}