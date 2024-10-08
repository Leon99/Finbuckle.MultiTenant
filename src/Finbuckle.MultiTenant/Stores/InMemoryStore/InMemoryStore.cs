// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Collections.Concurrent;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.Stores.InMemoryStore;

/// <summary>
/// Basic store that keeps tenants in memory.
/// </summary>
/// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
public class InMemoryStore<TTenantInfo> : IMultiTenantStore<TTenantInfo>
    where TTenantInfo : class, ITenantInfo, new()
{
    readonly ConcurrentDictionary<string, TTenantInfo> _tenantMap;
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    readonly InMemoryStoreOptions<TTenantInfo> _options;

    /// <summary>
    /// Constructor for InMemoryStore.
    /// </summary>
    /// <param name="options">InMemoryStoreOptions instance for desired behavior.</param>
    /// <exception cref="MultiTenantException"></exception>
    public InMemoryStore(IOptions<InMemoryStoreOptions<TTenantInfo>> options)
    {
        _options = options.Value;

        var stringComparer = StringComparer.OrdinalIgnoreCase;
        if(_options.IsCaseSensitive)
            stringComparer = StringComparer.Ordinal;

        _tenantMap = new ConcurrentDictionary<string, TTenantInfo>(stringComparer);
        foreach(var tenant in _options.Tenants)
        {
            if(String.IsNullOrWhiteSpace(tenant.Id))
                throw new MultiTenantException("Missing tenant id in options.");
            if(String.IsNullOrWhiteSpace(tenant.Key))
                throw new MultiTenantException("Missing tenant identifier in options.");
            if(_tenantMap.ContainsKey(tenant.Key))
                throw new MultiTenantException("Duplicate tenant identifier in options.");

            _tenantMap.TryAdd(tenant.Key, tenant);
        }
    }

    /// <inheritdoc />
    public async Task<TTenantInfo?> TryGetAsync(string id)
    {
        var result = _tenantMap.Values.SingleOrDefault(ti => ti.Id == id);
        return await Task.FromResult(result);
    }

    /// <inheritdoc />
    public async Task<TTenantInfo?> TryGetByKeyAsync(string key)
    {
        _tenantMap.TryGetValue(key, out var result);

        return await Task.FromResult(result);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TTenantInfo>> GetAllAsync()
    {
        return await Task.FromResult(_tenantMap.Select(x => x.Value).ToList());
    }

    /// <inheritdoc />
    public async Task<bool> TryAddAsync(TTenantInfo tenantInfo)
    {
        var result = tenantInfo.Key is not null && _tenantMap.TryAdd(tenantInfo.Key, tenantInfo);

        return await Task.FromResult(result);
    }

    /// <inheritdoc />
    public async Task<bool> TryRemoveAsync(string key)
    {
        var result = _tenantMap.TryRemove(key, out var _);

        return await Task.FromResult(result);
    }

    /// <inheritdoc />
    public async Task<bool> TryUpdateAsync(TTenantInfo tenantInfo)
    {
        var existingTenantInfo = tenantInfo.Id is not null ? await TryGetAsync(tenantInfo.Id) : null;

        if (existingTenantInfo?.Key is not null)
        {
            var result =  _tenantMap.TryUpdate(existingTenantInfo.Key, tenantInfo, existingTenantInfo);
            return await Task.FromResult(result);
        }

        return await Task.FromResult(false);
    }
}