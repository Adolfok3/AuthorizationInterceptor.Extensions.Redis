using AuthorizationInterceptor.Builder;
using AuthorizationInterceptor.Extensions.Redis.Interceptors;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AuthorizationInterceptor.Extensions.Redis.Extensions
{
    /// <summary>
    /// Extension methods that Configures the authorization interceptor to use a Redis distributed cache interceptor for <see cref="IAuthorizationInterceptorBuilder"/>
    /// </summary>
    public static class AuthorizationInterceptorBuilderExtensions
    {
        /// <summary>
        /// Configures the authorization interceptor to use an Redis distributed cache interceptor.
        /// </summary>
        /// <param name="builder"><see cref="IAuthorizationInterceptorBuilder"/></param>
        /// <param name="options"><see cref="RedisCacheOptions"/></param>
        /// <returns><see cref="IAuthorizationInterceptorBuilder"/></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IAuthorizationInterceptorBuilder AddStackExchangeRedisCache(this IAuthorizationInterceptorBuilder builder, Action<RedisCacheOptions> options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            builder.HttpClientBuilder.Services.AddStackExchangeRedisCache(options);

            builder.AddCustom<RedisAuthorizationInterceptor>();
            return builder;
        }
    }
}
