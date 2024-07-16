// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Strategies;

public class DelegateStrategy(Func<object, Task<string?>> doStrategy) : IMultiTenantStrategy
{
    private readonly Func<object, Task<string?>> _doStrategy = doStrategy ?? throw new ArgumentNullException(nameof(doStrategy));

    public async Task<string?> GetIdentifierAsync(object context)
    {
        var identifier = await _doStrategy(context);
        return await Task.FromResult(identifier);
    }
}