using AuthorizationInterceptor.Builder;
using AuthorizationInterceptor.Extensions.Redis.Extensions;
using AuthorizationInterceptor.Extensions.Redis.Interceptors;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;

namespace AuthorizationInterceptor.Extensions.Redis.Tests.Extensions;

public class AuthorizationInterceptorBuilderExtensionsTests
{
    [Fact]
    public void AddStackExchangeRedisCache_ShouldRegisterDistributedCacheDependencies_AndAddInterceptor()
    {
        // Arrange
        var services = new ServiceCollection();
        var httpBuilder = Substitute.For<IHttpClientBuilder>();
        httpBuilder.Services.Returns(services);
        var builder = new AuthorizationInterceptorBuilder(httpBuilder, null, null);

        // Act
        builder.AddStackExchangeRedisCache(opt =>
        {
            opt.InstanceName = "test";
            opt.Configuration = "test";
        });

        // Assert
        Assert.Single(services, service => service.ServiceType == typeof(IDistributedCache));

        var fieldInfo = builder.GetType().GetField("_interceptors", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var interceptors = (List<Type>)fieldInfo.GetValue(builder);

        Assert.Contains(typeof(RedisAuthorizationInterceptor), interceptors);
    }

    [Fact]
    public void AddStackExchangeRedisCache_WithOptionsNull_ShouldThrows()
    {
        // Arrange
        var services = new ServiceCollection();
        var httpBuilder = Substitute.For<IHttpClientBuilder>();
        httpBuilder.Services.Returns(services);
        var builder = new AuthorizationInterceptorBuilder(httpBuilder, null, null);

        // Act
        var act = () => builder.AddStackExchangeRedisCache(null);

        // Assert
        Assert.Equal("Value cannot be null. (Parameter 'options')", Assert.Throws<ArgumentNullException>(act).Message);
    }
}
