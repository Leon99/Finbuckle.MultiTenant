// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Options;
using Finbuckle.MultiTenant.AspNetCore.Strategies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Finbuckle.MultiTenant.AspNetCore.Test.Strategies;

public class BasePathStrategyShould
{
    HttpContext CreateHttpContextMock(string path, string pathBase = "/")
    {
        var mock = new Mock<HttpContext>();
        mock.SetupProperty<PathString>(c => c.Request.Path, path);
        mock.SetupProperty<PathString>(c => c.Request.PathBase, pathBase);
        mock.SetupProperty(c => c.RequestServices);
        return mock.Object;
    }

    [Fact]
    public async void RebaseAspNetCoreBasePathIfOptionTrue()
    {

        var services = new ServiceCollection();
        services.AddOptions().AddMultiTenant<TenantInfo>().WithBasePathStrategy().WithInMemoryStore(options =>
        {
            options.Tenants.Add(new TenantInfo
            {
                Id = "base123",
                Key = "base",
                Name = "base tenant"
            });
        });
        services.Configure<BasePathStrategyOptions>(options => options.RebaseAspNetCorePathBase = true);
        var serviceProvider = services.BuildServiceProvider();
        var httpContext = CreateHttpContextMock("/base/notBase");
        httpContext.RequestServices = serviceProvider;

        Assert.Equal("/", httpContext.Request.PathBase);
        Assert.Equal("/base/notBase", httpContext.Request.Path);
            
        // will trigger OnTenantFound event...
        var resolver = await serviceProvider.GetRequiredService<ITenantResolver>().ResolveAsync(httpContext);

        Assert.Equal("/base", httpContext.Request.PathBase);
        Assert.Equal("/notBase", httpContext.Request.Path);
    }
        
    [Fact]
    public async void NotRebaseAspNetCoreBasePathIfOptionFalse()
    {

        var services = new ServiceCollection();
        services.AddOptions().AddMultiTenant<TenantInfo>().WithBasePathStrategy().WithInMemoryStore(options =>
        {
            options.Tenants.Add(new TenantInfo
            {
                Id = "base123",
                Key = "base",
                Name = "base tenant"
            });
        });
        services.Configure<BasePathStrategyOptions>(options => options.RebaseAspNetCorePathBase = false);
        var serviceProvider = services.BuildServiceProvider();
        var httpContext = CreateHttpContextMock("/base/notBase");
        httpContext.RequestServices = serviceProvider;

        Assert.Equal("/", httpContext.Request.PathBase);
        Assert.Equal("/base/notBase", httpContext.Request.Path);
            
        // will trigger OnTenantFound event...
        var resolver = await serviceProvider.GetRequiredService<ITenantResolver>().ResolveAsync(httpContext);

        Assert.Equal("/", httpContext.Request.PathBase);
        Assert.Equal("/base/notBase", httpContext.Request.Path);
    }

    [Theory]
    [InlineData("/test", "test")] // single path
    [InlineData("/Test", "Test")] // maintain case
    [InlineData("", null)] // no path
    [InlineData("/", null)] // just trailing slash
    [InlineData("/initech/ignore/ignore", "initech")] // multiple path segments
    public async void ReturnExpectedIdentifier(string path, string? expected)
    {
        var httpContext = CreateHttpContextMock(path);
        var strategy = new BasePathStrategy();

        var identifier = await strategy.GetKeyAsync(httpContext);

        Assert.Equal(expected, identifier);
    }

    [Fact]
    public async void ThrowIfContextIsNotHttpContext()
    {
        var context = new Object();
        var strategy = new BasePathStrategy();

        await Assert.ThrowsAsync<MultiTenantException>(() => strategy.GetKeyAsync(context));
    }

    [Fact]
    public async void AppendTenantToExistingBase()
    {

        var services = new ServiceCollection();
        services.AddOptions().AddMultiTenant<TenantInfo>().WithBasePathStrategy().WithInMemoryStore(options =>
        {
            options.Tenants.Add(new TenantInfo
            {
                Id = "tenant",
                Key = "tenant",
                Name = "tenant"
            });
        });
        services.Configure<BasePathStrategyOptions>(options => options.RebaseAspNetCorePathBase = true);
        var serviceProvider = services.BuildServiceProvider();
        var httpContext = CreateHttpContextMock("/tenant/path", "/base");
        httpContext.RequestServices = serviceProvider;

        Assert.Equal("/base", httpContext.Request.PathBase);
        Assert.Equal("/tenant/path", httpContext.Request.Path);

        // will trigger OnTenantFound event...
        var resolver = await serviceProvider.GetRequiredService<ITenantResolver>().ResolveAsync(httpContext);

        Assert.Equal("/base/tenant", httpContext.Request.PathBase);
        Assert.Equal("/path", httpContext.Request.Path);
    }
}