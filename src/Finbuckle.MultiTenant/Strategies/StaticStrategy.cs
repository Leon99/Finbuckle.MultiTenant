// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Strategies;

public class StaticStrategy(string key) : IMultiTenantStrategy
{
    internal readonly string Key = key;

    public int Priority => -1000;

    /// <inheritdoc />
    public Task<string> GetKeyAsync(object context)
    {
        return Task.FromResult(Key);
    }
}