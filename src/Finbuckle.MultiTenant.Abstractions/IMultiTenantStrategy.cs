// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Determines the tenant key.
/// </summary>
public interface IMultiTenantStrategy
{
    /// <summary>
    /// Gets the key of the current tenant.
    /// </summary>
    /// <param name="context">The context object used to determine the key.</param>
    /// <returns>The found identifier or null.</returns>
    Task<string> GetKeyAsync(object context);
    
    /// <summary>
    /// Sets the key of the current tenant.
    /// </summary>
    /// <param name="context">The context object used to determine the key.</param>
    Task SetKeyAsync(object context, string key) => throw new NotImplementedException();

    /// <summary>
    /// Strategy execution order priority. Low values are executed first. Equal values are executed in order of registration.
    /// </summary>
    int Priority => 0;
}