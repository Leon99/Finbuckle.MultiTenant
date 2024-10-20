// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Interface for basic tenant information.
/// </summary>
public interface ITenantInfo
{
    /// <summary>
    /// Gets or sets a unique Id for the tenant.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="Key"/>, Id is never intended to be changed.
    /// </remarks>
    string? Id { get; set; }
    
    /// <summary>
    /// Gets or sets a unique key for the tenant.
    /// </summary>
    /// <remarks>
    /// It is intended for use during tenant resolution and format is determined by convention. For example
    /// a web based strategy may require URL friendly identifiers. Keys can be changed if needed.
    /// </remarks>
    string? Key { get; set;  }
    
    /// <summary>
    /// Gets or sets a display friendly name for the tenant.
    /// </summary>
    string? Name { get; set; }
}