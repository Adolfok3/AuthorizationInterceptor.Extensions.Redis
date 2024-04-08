using AuthorizationInterceptor.Entries;
using AuthorizationInterceptor.Extensions.Redis.Interceptors;
using AuthorizationInterceptor.Handlers;
using AuthorizationInterceptor.Interceptors;
using AuthorizationInterceptor.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AuthorizationInterceptor.Extensions.Redis.Tests.Interceptors;

public class RedisAuthorizationInterceptorTests
{
    private const string KEY = "authorization_interceptor_redis_cache_ObjectProxy";
    private readonly IAuthenticationHandler _authentication;
    private readonly IDistributedCache _cache;
    private readonly ILogger<AuthorizationInterceptorBase> _logger;
    private RedisAuthorizationInterceptor _interceptor;

    public RedisAuthorizationInterceptorTests()
    {
        _authentication = Substitute.For<IAuthenticationHandler>();
        _cache = Substitute.For<IDistributedCache>();
        _logger = Substitute.For<ILogger<AuthorizationInterceptorBase>>();
        _interceptor = new RedisAuthorizationInterceptor(_authentication, _cache, _logger, null);
    }

    [Fact]
    public async Task GetHeadersAsync_ShouldGetFromInner()
    {
        //Arrange
        var entries = new AuthorizationEntry(TimeSpan.FromMinutes(3))
        {
            { "Authorization", "Bearer token" }
        };
        _authentication.AuthenticateAsync().Returns(entries);

        //Act
        var headers = await _interceptor.GetHeadersAsync();

        //
        Assert.NotNull(headers);
        Assert.NotEqual(DateTimeOffset.MinValue, headers.AuthenticatedAt);
        Assert.Equal(TimeSpan.FromMinutes(3), headers.ExpiresIn);
        Assert.Contains(headers, a => a.Key == "Authorization" && a.Value == "Bearer token");
        await _authentication.Received(1).AuthenticateAsync();
        await _cache.Received(1).GetAsync(KEY);
        await _cache.Received(1).SetAsync(KEY, Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>(), default);
    }

    [Fact]
    public async Task GetHeadersAsync_ShouldGetFromCache()
    {
        //Arrange
        var entries = new AuthorizationEntry(TimeSpan.FromMinutes(3))
        {
            { "Authorization", "Bearer token" }
        };
        var json = AuthorizationEntryJsonSerializer.Serialize(entries);
        var bytes = Encoding.UTF8.GetBytes(json);
        _cache.GetAsync(KEY).Returns(bytes);

        //Act
        var headers = await _interceptor.GetHeadersAsync();

        //
        Assert.NotNull(headers);
        Assert.NotEqual(DateTimeOffset.MinValue, headers.AuthenticatedAt);
        Assert.Equal(TimeSpan.FromMinutes(3), headers.ExpiresIn);
        Assert.Contains(headers, a => a.Key == "Authorization" && a.Value == "Bearer token");
        await _authentication.Received(0).AuthenticateAsync();
        await _cache.Received(1).GetAsync(KEY);
        await _cache.Received(0).SetAsync(KEY, Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>(), default);
    }

    [Fact]
    public async Task GetHeadersAsync_WithOAuth_ShouldGetFromCache()
    {
        //Arrange
        AuthorizationEntry entries = new OAuthEntry("toke", "type", 123, "refresh", "scope");
        var json = AuthorizationEntryJsonSerializer.Serialize(entries);
        var bytes = Encoding.UTF8.GetBytes(json);
        _cache.GetAsync(KEY).Returns(bytes);

        //Act
        var headers = await _interceptor.GetHeadersAsync();

        //Assert
        Assert.NotNull(headers);
        Assert.NotEmpty(headers);
        Assert.NotEqual(DateTimeOffset.MinValue, headers.AuthenticatedAt);
        Assert.Equal(TimeSpan.FromSeconds(123), headers.ExpiresIn);
        Assert.Contains(headers, a => a.Key == "Authorization" && a.Value == "type toke");
        Assert.NotNull(headers.OAuthEntry);
        await _authentication.Received(0).AuthenticateAsync();
        await _cache.Received(1).GetAsync(KEY);
        await _cache.Received(0).SetAsync(KEY, Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>(), default);
    }
}