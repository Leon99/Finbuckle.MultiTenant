// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant.AspNetCore.Strategies;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class HeaderStrategy(string headerKey) : IMultiTenantStrategy
{
    public Task<string?> GetKeyAsync(object context)
    {
        if (!(context is HttpContext httpContext))
            throw new MultiTenantException(null,
                new ArgumentException($"\"{nameof(context)}\" type must be of type HttpContext", nameof(context)));

        return Task.FromResult(httpContext?.Request.Headers[headerKey].FirstOrDefault());
    }
}