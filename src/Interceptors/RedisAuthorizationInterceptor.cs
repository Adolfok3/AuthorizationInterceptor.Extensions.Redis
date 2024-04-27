using AuthorizationInterceptor.Extensions.Abstractions.Headers;
using AuthorizationInterceptor.Extensions.Abstractions.Interceptors;
using AuthorizationInterceptor.Extensions.Abstractions.Json;
using Microsoft.Extensions.Caching.Distributed;
using System.Threading.Tasks;

namespace AuthorizationInterceptor.Extensions.Redis.Interceptors
{
    internal class RedisAuthorizationInterceptor : IAuthorizationInterceptor
    {
        private readonly IDistributedCache _cache;
        private const string CACHE_KEY = "authorization_interceptor_redis_cache_RedisAuthorizationInterceptor_{0}";

        public RedisAuthorizationInterceptor(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<AuthorizationHeaders?> GetHeadersAsync(string name)
        {
            var data = await _cache.GetStringAsync(string.Format(CACHE_KEY, name));
            if (string.IsNullOrEmpty(data))
                return null;

            return AuthorizationHeadersJsonSerializer.Deserialize(data);
        }

        public async Task UpdateHeadersAsync(string name, AuthorizationHeaders? _, AuthorizationHeaders? newHeaders)
        {
            if (newHeaders == null)
                return;

            var data = AuthorizationHeadersJsonSerializer.Serialize(newHeaders);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = newHeaders.GetRealExpiration()
            };

            await _cache.SetStringAsync(string.Format(CACHE_KEY, name), data, options);
        }
    }
}
