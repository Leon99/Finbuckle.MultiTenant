// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant.AspNetCore.Strategies;

public class SessionStrategy(string tenantKey) : IMultiTenantStrategy
{
    readonly string _tenantKey = tenantKey ?? throw new ArgumentNullException(nameof(tenantKey));

    /// <inheritdoc />
    public Task<string> GetKeyAsync(object context)
    {
        var httpContext = EnsureHttpContext(context);

        return Task.FromResult(
            httpContext.Session.GetString(_tenantKey));
    }

    static HttpContext EnsureHttpContext(object context)
    {
        if (context is not HttpContext httpContext)
            throw new MultiTenantException(null,
                new ArgumentException($"\"{nameof(context)}\" type must be of type HttpContext", nameof(context)));
        return httpContext;
    }

    /// <inheritdoc />
    public Task SetKeyAsync(object context, string key)
    {
        var httpContext = EnsureHttpContext(context);

        httpContext.Session.SetString(_tenantKey, key);
        
        return Task.CompletedTask;
    }
}