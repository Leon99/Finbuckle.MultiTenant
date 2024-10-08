﻿// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Internal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.AspNetCore.Internal;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal class MultiTenantAuthenticationService<TTenantInfo>(
    IAuthenticationService inner,
    IOptionsMonitor<MultiTenantAuthenticationOptions> multiTenantAuthenticationOptions)
    : IAuthenticationService
    where TTenantInfo : class, ITenantInfo, new()
{
    readonly IAuthenticationService _inner = inner ?? throw new System.ArgumentNullException(nameof(inner));

    static void AddTenantIdentifierToProperties(HttpContext context, ref AuthenticationProperties? properties)
    {
            // Add tenant identifier to the properties so on the callback we can use it to set the multitenant context.
            var multiTenantContext = context.GetMultiTenantContext<TTenantInfo>();
            if (multiTenantContext?.TenantInfo is not null)
            {
                properties ??= new AuthenticationProperties();
                if(!properties.Items.Keys.Contains(Constants.TenantToken))
                    properties.Items.Add(Constants.TenantToken, multiTenantContext.TenantInfo.Key);
            }
    }

    public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
        => _inner.AuthenticateAsync(context, scheme);

    public async Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
    {
        if (multiTenantAuthenticationOptions.CurrentValue.SkipChallengeIfTenantNotResolved)
        {
            if (context.GetMultiTenantContext<TTenantInfo>()?.TenantInfo is null)
                return;
        }

        AddTenantIdentifierToProperties(context, ref properties);
        await _inner.ChallengeAsync(context, scheme, properties);
    }

    public async Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
    {
        AddTenantIdentifierToProperties(context, ref properties);
        await _inner.ForbidAsync(context, scheme, properties);
    }

    public async Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
    {
        AddTenantIdentifierToProperties(context, ref properties);
        await _inner.SignInAsync(context, scheme, principal, properties);
    }

    public async Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
    {
        AddTenantIdentifierToProperties(context, ref properties);
        await _inner.SignOutAsync(context, scheme, properties);
    }
}