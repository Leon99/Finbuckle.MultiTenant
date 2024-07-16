// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Strategies;

public class StaticStrategy(string identifier) : IMultiTenantStrategy
{
    internal readonly string Identifier = identifier;

    public int Priority => -1000;

    public async Task<string?> GetIdentifierAsync(object context)
    {
        return await Task.FromResult(Identifier);
    }
}