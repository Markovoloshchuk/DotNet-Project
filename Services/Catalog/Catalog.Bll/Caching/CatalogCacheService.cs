
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Catalog.Bll.Caching;

public sealed class CatalogCacheService : ICatalogCacheService
{
    private static readonly TimeSpan L1SlidingExpiration =
        TimeSpan.FromSeconds(15);

    private static readonly TimeSpan L1AbsoluteExpiration =
        TimeSpan.FromSeconds(30);

    private static readonly TimeSpan L2AbsoluteExpiration =
        TimeSpan.FromMinutes(5);

    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<CatalogCacheService> _logger;

    public CatalogCacheService(
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        ILogger<CatalogCacheService> logger)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(
        string key,
        CancellationToken ct = default)
        where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        // L1: локальний кеш у пам'яті.
        if (_memoryCache.TryGetValue(key, out var memoryValue)
            && memoryValue is T cachedValue)
        {
            _logger.LogDebug(
                "L1 cache HIT for key {CacheKey}",
                key);

            return cachedValue;
        }

        _logger.LogDebug(
            "L1 cache MISS for key {CacheKey}",
            key);

        // L2: розподілений Redis-кеш.
        try
        {
            var cachedBytes = await _distributedCache.GetAsync(
                key,
                ct);

            if (cachedBytes is null || cachedBytes.Length == 0)
            {
                _logger.LogDebug(
                    "L2 cache MISS for key {CacheKey}",
                    key);

                return null;
            }

            var distributedValue =
                JsonSerializer.Deserialize<T>(cachedBytes);

            if (distributedValue is null)
            {
                _logger.LogWarning(
                    "L2 cache returned an empty value for key {CacheKey}",
                    key);

                return null;
            }

            _logger.LogDebug(
                "L2 cache HIT for key {CacheKey}",
                key);

            // Заповнюємо L1 після успішного отримання з L2.
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
            // Redis недоступний: повертаємо MISS,
            // щоб BLL могла звернутися до бази даних.
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

        // Спочатку заповнюємо L1.
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

            // Потім записуємо дані в L2.
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
            // Навіть якщо Redis недоступний,
            // локальний кеш залишається працездатним.
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

        // Інвалідуємо локальний кеш.
        _memoryCache.Remove(key);

        _logger.LogInformation(
            "L1 cache entry invalidated for key {CacheKey}",
            key);

        try
        {
            // Інвалідуємо розподілений кеш.
            await _distributedCache.RemoveAsync(
                key,
                ct);

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
            // Не допускаємо падіння основної операції
            // через недоступність Redis.
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
}
