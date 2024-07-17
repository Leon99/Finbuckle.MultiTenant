// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Internal;

namespace Finbuckle.MultiTenant;

public class TenantInfo : ITenantInfo
{
    private string? _id;

    public string? Id
    {
        get => _id;
        set
        {
            if (value != null)
            {
                if (value.Length > Constants.TenantIdMaxLength)
                {
                    throw new MultiTenantException($"The tenant id cannot exceed {Constants.TenantIdMaxLength} characters.");
                }
                _id = value;
            }
        }
    }

    public virtual string? Key { get; set; }
    public virtual string? Name { get; set; }
}