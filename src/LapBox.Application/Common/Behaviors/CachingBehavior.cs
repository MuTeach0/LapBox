using LapBox.Application.Common.Interfaces.Caching;
using LapBox.Domain.Common.Results.Abstractions;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Common.Behaviors;

public class CachingBehavior<TRequest, TResponse>(
    HybridCache cache,
    ILogger<CachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly HybridCache _cache = cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (request is not ICachedQuery cachedRequest)
        {
            return await next(ct);
        }

        _logger.LogInformation("Checking cache for {RequestName}", typeof(TRequest).Name);

        var result = await _cache.GetOrCreateAsync<TResponse>(
            cachedRequest.CacheKey,
            async _ =>
            {
                var response = await next(ct);
                return response;
            },
            new HybridCacheEntryOptions
            {
                Flags = HybridCacheEntryFlags.DisableUnderlyingData
            },
            cancellationToken: ct);

        // If cached value was null (cache miss → next() returned null → cached null),
        // re-execute next() to get the real result
        if (result is null || (result is IResult r && !r.IsSuccess))
        {
            var fresh = await next(ct);

            if (fresh is IResult res && res.IsSuccess)
            {
                _logger.LogInformation("Caching result for {RequestName}", typeof(TRequest).Name);

                await _cache.SetAsync(
                    cachedRequest.CacheKey,
                    fresh,
                    new HybridCacheEntryOptions
                    {
                        Expiration = cachedRequest.Expiration
                    },
                    cachedRequest.Tags,
                    ct);
            }

            return fresh;
        }

        _logger.LogInformation("Cache hit for {RequestName}", typeof(TRequest).Name);
        return result;
    }
}