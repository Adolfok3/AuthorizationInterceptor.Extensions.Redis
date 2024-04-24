![AuthorizationInterceptor Icon](./resources/icon.png)

# AuthorizationInterceptor.Extensions.Redis
[![GithubActions](https://github.com/Adolfok3/AuthorizationInterceptor.Extensions.Redis/actions/workflows/main.yml/badge.svg)](https://github.com/Adolfok3/AuthorizationInterceptor.Extensions.Redis/actions)
[![License](https://img.shields.io/badge/license-MIT-green)](./LICENSE)
[![Coverage Status](https://coveralls.io/repos/github/Adolfok3/AuthorizationInterceptor.Extensions.Redis/badge.svg?branch=main)](https://coveralls.io/github/Adolfok3/AuthorizationInterceptor.Extensions.Redis?branch=main)
[![NuGet Version](https://img.shields.io/nuget/vpre/AuthorizationInterceptor.Extensions.Redis)](https://www.nuget.org/packages/AuthorizationInterceptor.Extensions.Redis)

An interceptor for [AuthorizationInterceptor](https://github.com/Adolfok3/AuthorizationInterceptor) that uses a distributed cache with Redis to handle authorization headers. For more information on how to configure and use Authorization Interceptor, please check the main page of [AuthorizationInterceptor](https://github.com/Adolfok3/AuthorizationInterceptor).

### Installation
Run the following command in package manager console:
```
PM> Install-Package AuthorizationInterceptor.Extensions.Redis
```

Or from the .NET CLI as:
```
dotnet add package AuthorizationInterceptor.Extensions.Redis
```

### Setup
When adding Authorization Interceptor Handler, call the extension method `UseStackExchangeRedisCacheInterceptor` to options passing the redis options:
```csharp
services.AddHttpClient("TargetApi")
        .AddAuthorizationInterceptorHandler<TargetApiAuthClass>(options =>
		{
			options.UseStackExchangeRedisCacheInterceptor(redisOptions => 
			{
				redisOptions.Configuration = "redisConnStr";
				redisOptions.InstanceName = "target_api";
			});
		})
```
