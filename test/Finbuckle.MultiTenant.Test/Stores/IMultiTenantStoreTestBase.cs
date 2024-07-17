// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Xunit;

#pragma warning disable xUnit1013 // Public method should be marked as test

namespace Finbuckle.MultiTenant.Test.Stores;

// TODO convert these to async

public abstract class MultiTenantStoreTestBase
{
    protected abstract IMultiTenantStore<TenantInfo> CreateTestStore();

    protected virtual IMultiTenantStore<TenantInfo> PopulateTestStore(IMultiTenantStore<TenantInfo> store)
    {
        store.TryAddAsync(new TenantInfo { Id = "initech-id", Key = "initech", Name = "Initech" }).Wait();
        store.TryAddAsync(new TenantInfo { Id = "lol-id", Key = "lol", Name = "Lol, Inc." }).Wait();

        return store;
    }

    //[Fact]
    public virtual void GetTenantInfoFromStoreById()
    {
        var store = CreateTestStore();

        Assert.Equal("initech", store.TryGetAsync("initech-id").Result!.Key);
    }

    //[Fact]
    public virtual void ReturnNullWhenGettingByIdIfTenantInfoNotFound()
    {
        var store = CreateTestStore();

        Assert.Null(store.TryGetAsync("fake123").Result);
    }

    //[Fact]
    public virtual void GetTenantInfoFromStoreByIdentifier()
    {
        var store = CreateTestStore();

        Assert.Equal("initech", store.TryGetByKeyAsync("initech").Result!.Key);
    }

    //[Fact]
    public virtual void ReturnNullWhenGettingByIdentifierIfTenantInfoNotFound()
    {
        var store = CreateTestStore();
        Assert.Null(store.TryGetByKeyAsync("fake123").Result);
    }

    //[Fact]
    public virtual void AddTenantInfoToStore()
    {
        var store = CreateTestStore();

        Assert.Null(store.TryGetByKeyAsync("identifier").Result);
        Assert.True(store.TryAddAsync(new TenantInfo
            { Id = "id", Key = "identifier", Name = "name" }).Result);
        Assert.NotNull(store.TryGetByKeyAsync("identifier").Result);
    }

    //[Fact]
    public virtual void UpdateTenantInfoInStore()
    {
        var store = CreateTestStore();

        var result = store.TryUpdateAsync(new TenantInfo
            { Id = "initech-id", Key = "initech2", Name = "Initech2" }).Result;
        Assert.True(result);
    }

    //[Fact]
    public virtual void RemoveTenantInfoFromStore()
    {
        var store = CreateTestStore();
        Assert.NotNull(store.TryGetByKeyAsync("initech").Result);
        Assert.True(store.TryRemoveAsync("initech").Result);
        Assert.Null(store.TryGetByKeyAsync("initech").Result);
    }

    //[Fact]
    public virtual void GetAllTenantsFromStoreAsync()
    {
        var store = CreateTestStore();
        Assert.Equal(2, store.GetAllAsync().Result.Count());
    }
}