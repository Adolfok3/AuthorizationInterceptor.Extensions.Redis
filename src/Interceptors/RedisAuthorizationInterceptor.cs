using AuthorizationInterceptor.Entries;
using AuthorizationInterceptor.Handlers;
using AuthorizationInterceptor.Interceptors;
using AuthorizationInterceptor.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AuthorizationInterceptor.Extensions.Redis.Interceptors
{
    internal class RedisAuthorizationInterceptor : AuthorizationInterceptorBase
    {
        private readonly IDistributedCache _cache;
        private readonly string _cacheKey;

        public RedisAuthorizationInterceptor(IAuthenticationHandler authenticationHandler, IDistributedCache cache, ILogger<AuthorizationInterceptorBase> logger, IAuthorizationInterceptor? nextInterceptor = null) : base("RedisCache", authenticationHandler, logger, nextInterceptor)
        {
            _cache = cache;
            _cacheKey = $"authorization_interceptor_redis_cache_{GetAuthenticationHandlerName()}";
        }

        protected override async Task<AuthorizationEntry> OnGetHeadersAsync()
        {
            var headers = await GetHeadersFromCache();
            if (headers != null)
                return headers;

            headers = await base.OnGetHeadersAsync();
            await SetHeadersToCache(headers);

            return headers;
        }

        protected override async Task<AuthorizationEntry> OnUpdateHeadersAsync(AuthorizationEntry expiredEntries)
        {
            await _cache.RemoveAsync(_cacheKey);
            var newHeaders = await base.OnUpdateHeadersAsync(expiredEntries);
            Log("Setting new headers on RedisCache");

            await SetHeadersToCache(newHeaders);
            return newHeaders;
        }

        private async Task<AuthorizationEntry?> GetHeadersFromCache()
        {
            var data = await _cache.GetStringAsync(_cacheKey);
            if (string.IsNullOrEmpty(data))
                return null;

            return AuthorizationEntryJsonSerializer.Deserialize(data);
        }

        private async Task SetHeadersToCache(AuthorizationEntry headers)
        {
            var data = AuthorizationEntryJsonSerializer.Serialize(headers);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = headers.GetRealExpiration()
            };

            await _cache.SetStringAsync(_cacheKey, data, options);
        }
    }
}
