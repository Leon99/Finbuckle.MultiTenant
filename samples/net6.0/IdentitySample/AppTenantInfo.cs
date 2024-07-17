using Finbuckle.MultiTenant.Abstractions;

namespace IdentitySample;

public class AppTenantInfo : ITenantInfo
{
    public string? Id { get; set; }
    public string? Key { get; set; }
    public string? Name { get; set; }
    public string? ConnectionString { get; set; }
}