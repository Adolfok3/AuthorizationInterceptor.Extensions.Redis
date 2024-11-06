using AuthorizationInterceptor.Extensions.Abstractions.Options;
using AuthorizationInterceptor.Extensions.Redis.Interceptors;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AuthorizationInterceptor.Extensions.Redis.Extensions
{
    /// <summary>
    /// Extension methods that Configures the authorization interceptor to use a Redis distributed cache interceptor for <see cref="IAuthorizationInterceptorOptions"/>
    /// </summary>
    public static class AuthorizationInterceptorOptionsExtensions
    {
        /// <summary>
        /// Configures the authorization interceptor to use a Redis distributed cache interceptor.
        /// </summary>
        /// <param name="options"><see cref="IAuthorizationInterceptorOptions"/></param>
        /// <param name="optionsRedis"><see cref="RedisCacheOptions"/></param>
        /// <returns><see cref="IAuthorizationInterceptorOptions"/></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IAuthorizationInterceptorOptions UseStackExchangeRedisCacheInterceptor(this IAuthorizationInterceptorOptions options, Action<RedisCacheOptions> optionsRedis)
        {
            if (optionsRedis == null)
                throw new ArgumentNullException(nameof(optionsRedis));

            options.UseCustomInterceptor<RedisAuthorizationInterceptor>(s => s.AddStackExchangeRedisCache(optionsRedis));

            return options;
        }
    }
}
