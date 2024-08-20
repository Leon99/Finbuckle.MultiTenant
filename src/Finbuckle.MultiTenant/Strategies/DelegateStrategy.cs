// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Strategies;

public class DelegateStrategy(Func<object, Task<string?>> doStrategy) : IMultiTenantStrategy
{
    readonly Func<object, Task<string?>> _doStrategy = doStrategy ?? throw new ArgumentNullException(nameof(doStrategy));

    /// <inheritdoc />
    public async Task<string> GetKeyAsync(object context)
    {
        return await _doStrategy(context);
    }
}