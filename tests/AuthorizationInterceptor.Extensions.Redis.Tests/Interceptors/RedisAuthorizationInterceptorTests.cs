using AuthorizationInterceptor.Extensions.Abstractions.Headers;
using AuthorizationInterceptor.Extensions.Abstractions.Interceptors;
using AuthorizationInterceptor.Extensions.Abstractions.Json;
using AuthorizationInterceptor.Extensions.Redis.Interceptors;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AuthorizationInterceptor.Extensions.Redis.Tests.Interceptors;

public class RedisAuthorizationInterceptorTests
{
    private const string KEY = "authorization_interceptor_redis_cache_RedisAuthorizationInterceptor_test";
    private readonly IDistributedCache _cache;
    private IAuthorizationInterceptor _interceptor;

    public RedisAuthorizationInterceptorTests()
    {
        _cache = Substitute.For<IDistributedCache>();
        _interceptor = new RedisAuthorizationInterceptor(_cache);
    }

    [Fact]
    public async Task GetHeadersAsync_ShouldGetFromCache()
    {
        //Arrange
        var headers = new AuthorizationHeaders(TimeSpan.FromMinutes(3))
        {
            { "Authorization", "Bearer token" }
        };
        var json = AuthorizationHeadersJsonSerializer.Serialize(headers);
        var bytes = Encoding.UTF8.GetBytes(json);
        _cache.GetAsync(KEY).Returns(bytes);

        //Act
        headers = await _interceptor.GetHeadersAsync("test");

        //
        Assert.NotNull(headers);
        Assert.NotEqual(DateTimeOffset.MinValue, headers.AuthenticatedAt);
        Assert.Equal(TimeSpan.FromMinutes(3), headers.ExpiresIn);
        Assert.Contains(headers, a => a.Key == "Authorization" && a.Value == "Bearer token");
        await _cache.Received(1).GetAsync(KEY);
        await _cache.Received(0).SetAsync(KEY, Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>(), default);
    }

    [Fact]
    public async Task GetHeadersAsync_WithOAuth_ShouldGetFromCache()
    {
        //Arrange
        AuthorizationHeaders? headers = new OAuthHeaders("toke", "type", 123, "refresh", "scope");
        var json = AuthorizationHeadersJsonSerializer.Serialize(headers);
        var bytes = Encoding.UTF8.GetBytes(json);
        _cache.GetAsync(KEY).Returns(bytes);

        //Act
        headers = await _interceptor.GetHeadersAsync("test");

        //Assert
        Assert.NotNull(headers);
        Assert.NotEmpty(headers);
        Assert.NotEqual(DateTimeOffset.MinValue, headers.AuthenticatedAt);
        Assert.Equal(TimeSpan.FromSeconds(123), headers.ExpiresIn);
        Assert.Contains(headers, a => a.Key == "Authorization" && a.Value == "type toke");
        Assert.NotNull(headers.OAuthHeaders);
        await _cache.Received(1).GetAsync(KEY);
        await _cache.Received(0).SetAsync(KEY, Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>(), default);
    }

    [Fact]
    public async Task GetHeadersAsync_ShouldGetFromCache_AndReturnNull()
    {
        //Arrange
        _cache.GetAsync(KEY).Returns(Task.FromResult<byte[]?>(null));

        //Act
        var headers = await _interceptor.GetHeadersAsync("test");

        //
        Assert.Null(headers);
        await _cache.Received(1).GetAsync(KEY);
        await _cache.Received(0).SetAsync(KEY, Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>(), default);
    }

    [Fact]
    public async Task UpdateHeadersAsync_ShouldUpdateSuccessfuly()
    {
        //Arrange
        var headers = new AuthorizationHeaders(TimeSpan.FromMinutes(3))
        {
            { "Authorization", "Bearer token" }
        };

        //Act
        var act = () => _interceptor.UpdateHeadersAsync("test", null, headers);

        //
        Assert.Null(await Record.ExceptionAsync(act));
        await _cache.Received(1).SetAsync(KEY, Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>(), default);
    }

    [Fact]
    public async Task UpdateHeadersAsync_WithNullHeaders_ShouldNotUpdate()
    {
        //Act
        var act = () => _interceptor.UpdateHeadersAsync("test", null, null);

        //
        Assert.Null(await Record.ExceptionAsync(act));
        await _cache.Received(0).SetAsync(KEY, Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>(), default);
    }
}