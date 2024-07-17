// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant.AspNetCore.Strategies;

public class SessionStrategy(string tenantKey) : IMultiTenantStrategy
{
    private readonly string _tenantKey = tenantKey ?? throw new ArgumentNullException(nameof(tenantKey));

    public Task<string?> GetIdentifierAsync(object context)
    {
        if (context is not HttpContext httpContext)
            throw new MultiTenantException(null,
                new ArgumentException($"\"{nameof(context)}\" type must be of type HttpContext", nameof(context)));

        var identifier = httpContext.Session.GetString(_tenantKey);
        return Task.FromResult(identifier); // Prevent the compiler warning that no await exists.
    }
}