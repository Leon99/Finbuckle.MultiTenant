// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Stores.EFCoreStore;

public class EFCoreStoreDbContext<TTenantInfo>(DbContextOptions options)
    : DbContext(options), IEFCoreStoreDbContext<TTenantInfo>
    where TTenantInfo : class, ITenantInfo, new()
{
    public DbSet<TTenantInfo> TenantInfos => Set<TTenantInfo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureTenantInfoEntity<TTenantInfo>();
    }
}