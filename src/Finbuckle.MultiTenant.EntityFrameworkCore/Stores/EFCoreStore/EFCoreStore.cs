// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Stores.EFCoreStore;

public class EFCoreStore<TEFCoreStoreDbContext, TTenantInfo>(TEFCoreStoreDbContext dbContext)
    : IMultiTenantStore<TTenantInfo>
    where TEFCoreStoreDbContext : DbContext, IEFCoreStoreDbContext<TTenantInfo>
    where TTenantInfo : class, ITenantInfo, new()
{
    internal readonly TEFCoreStoreDbContext DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public virtual async Task<TTenantInfo?> TryGetAsync(string id)
    {
        return await DbContext.TenantInfos.AsNoTracking()
            .Where(ti => ti.Id == id)
            .SingleOrDefaultAsync();
    }

    public virtual async Task<IEnumerable<TTenantInfo>> GetAllAsync()
    {
        return await DbContext.TenantInfos.AsNoTracking().ToListAsync();
    }

    public virtual async Task<TTenantInfo?> TryGetByKeyAsync(string key)
    {
        return await DbContext.TenantInfos.AsNoTracking()
            .Where(ti => ti.Key == key)
            .SingleOrDefaultAsync();
    }

    public virtual async Task<bool> TryAddAsync(TTenantInfo tenantInfo)
    {
        await DbContext.TenantInfos.AddAsync(tenantInfo);
        var result = await DbContext.SaveChangesAsync() > 0;
        DbContext.Entry(tenantInfo).State = EntityState.Detached;
            
        return result;
    }

    public virtual async Task<bool> TryRemoveAsync(string key)
    {
        var existing = await DbContext.TenantInfos
            .Where(ti => ti.Key == key)
            .SingleOrDefaultAsync();

        if (existing is null)
        {
            return false;
        }

        DbContext.TenantInfos.Remove(existing);
        return await DbContext.SaveChangesAsync() > 0;
    }

    public virtual async Task<bool> TryUpdateAsync(TTenantInfo tenantInfo)
    {
        DbContext.TenantInfos.Update(tenantInfo);
        var result = await DbContext.SaveChangesAsync() > 0;
        DbContext.Entry(tenantInfo).State = EntityState.Detached;
        return result;
    }
}