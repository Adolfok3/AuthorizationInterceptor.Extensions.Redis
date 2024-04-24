using AuthorizationInterceptor.Extensions.Abstractions.Options;
using AuthorizationInterceptor.Extensions.Redis.Extensions;
using AuthorizationInterceptor.Extensions.Redis.Interceptors;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using Xunit;

namespace AuthorizationInterceptor.Extensions.Redis.Tests.Extensions;

public class AuthorizationInterceptorBuilderExtensionsTests
{
    [Fact]
    public void UseStackExchangeRedisCache_ShouldRegisterDistributedCacheDependencies_AndAddInterceptor()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = Substitute.For<IAuthorizationInterceptorOptions>();

        // Act
        options.UseStackExchangeRedisCacheInterceptor(opt =>
        {
            opt.InstanceName = "test";
            opt.Configuration = "test";
        });

        // Assert
        options.Received(1).UseCustomInterceptor<RedisAuthorizationInterceptor>(Arg.Any<Func<IServiceCollection, IServiceCollection>?>());
    }

    [Fact]
    public void UseStackExchangeRedisCache_WithOptionsNull_ShouldThrows()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = Substitute.For<IAuthorizationInterceptorOptions>();

        // Act
        var act = () => options.UseStackExchangeRedisCacheInterceptor(null);

        // Assert
        Assert.Equal("Value cannot be null. (Parameter 'optionsRedis')", Assert.Throws<ArgumentNullException>(act).Message);
    }
}
