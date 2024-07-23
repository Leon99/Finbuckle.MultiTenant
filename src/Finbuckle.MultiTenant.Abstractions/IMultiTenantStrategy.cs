// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Determines the tenant key.
/// </summary>
public interface IMultiTenantStrategy
{
    /// <summary>
    ///  Method for implementations to control how the key is determined.
    /// </summary>
    /// <param name="context">The context object used to determine the key.</param>
    /// <returns>The found identifier or null.</returns>
    Task<string> GetKeyAsync(object context);

    /// <summary>
    /// Strategy execution order priority. Low values are executed first. Equal values are executed in order of registration.
    /// </summary>
    int Priority => 0;
}