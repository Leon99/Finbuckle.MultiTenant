﻿// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant.AspNetCore.Strategies;

public class RouteStrategy : IMultiTenantStrategy
{
    internal readonly string TenantParam;

    public RouteStrategy(string tenantParam)
    {
        if (string.IsNullOrWhiteSpace(tenantParam))
        {
            throw new ArgumentException($"\"{nameof(tenantParam)}\" must not be null or whitespace",
                nameof(tenantParam));
        }

        this.TenantParam = tenantParam;
    }

    /// <inheritdoc />
    public Task<string> GetKeyAsync(object context)
    {
        if (!(context is HttpContext httpContext))
            throw new MultiTenantException(null,
                new ArgumentException($"\"{nameof(context)}\" type must be of type HttpContext", nameof(context)));

        httpContext.Request.RouteValues.TryGetValue(TenantParam, out var key);

        return Task.FromResult(key as string);
    }
}