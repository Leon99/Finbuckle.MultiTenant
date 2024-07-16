// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Text.Json;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Caching.Distributed;

namespace Finbuckle.MultiTenant.Stores.DistributedCacheStore;

/// <summary>
/// Basic store that uses an IDistributedCache instance as its backing. Note that GetAllAsync is not implemented.
/// </summary>
/// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
public class DistributedCacheStore<TTenantInfo> : IMultiTenantStore<TTenantInfo> where TTenantInfo : class, ITenantInfo, new()
{
    private readonly IDistributedCache _cache;
    private readonly string _keyPrefix;
    private readonly TimeSpan? _slidingExpiration;

    /// <summary>
    /// Constructor for DistributedCacheStore.
    /// </summary>
    /// <param name="cache">IDistributedCache instance for use as the store backing.</param>
    /// <param name="keyPrefix">Prefix string added to cache entries.</param>
    /// <param name="slidingExpiration">Amount of time to slide expiration with every access.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public DistributedCacheStore(IDistributedCache cache, string keyPrefix, TimeSpan? slidingExpiration)
    {
        this._cache = cache ?? throw new ArgumentNullException(nameof(cache));
        this._keyPrefix = keyPrefix ?? throw new ArgumentNullException(nameof(keyPrefix));
        this._slidingExpiration = slidingExpiration;
    }

    /// <inheritdoc />
    public async Task<bool> TryAddAsync(TTenantInfo tenantInfo)
    {
        var options = new DistributedCacheEntryOptions { SlidingExpiration = _slidingExpiration };
        var bytes = JsonSerializer.Serialize(tenantInfo);

        await _cache.SetStringAsync($"{_keyPrefix}id__{tenantInfo.Id}", bytes, options);
        await _cache.SetStringAsync($"{_keyPrefix}identifier__{tenantInfo.Identifier}", bytes, options);

        return true;
    }

    /// <inheritdoc />
    public async Task<TTenantInfo?> TryGetAsync(string id)
    {
        var bytes = await _cache.GetStringAsync($"{_keyPrefix}id__{id}");
        if (bytes == null)
            return null;

        var result = JsonSerializer.Deserialize<TTenantInfo>(bytes);

        // Refresh the identifier version to keep things synced
        await _cache.RefreshAsync($"{_keyPrefix}identifier__{result?.Identifier}");

        return result;
    }

    /// <summary>
    /// Not implemented in this implementation.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public Task<IEnumerable<TTenantInfo>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<TTenantInfo?> TryGetByIdentifierAsync(string identifier)
    {
        var bytes = await _cache.GetStringAsync($"{_keyPrefix}identifier__{identifier}");
        if (bytes == null)
            return null;

        var result = JsonSerializer.Deserialize<TTenantInfo>(bytes);

        // Refresh the identifier version to keep things synced
        await _cache.RefreshAsync($"{_keyPrefix}id__{result?.Id}");

        return result;
    }

    /// <inheritdoc />
    public async Task<bool> TryRemoveAsync(string identifier)
    {
        var result = await TryGetByIdentifierAsync(identifier);
        if (result == null)
            return false;

        await _cache.RemoveAsync($"{_keyPrefix}id__{result.Id}");
        await _cache.RemoveAsync($"{_keyPrefix}identifier__{result.Identifier}");

        return true;
    }

    /// <inheritdoc />
    public Task<bool> TryUpdateAsync(TTenantInfo tenantInfo)
    {
        // Same as adding for distributed cache.
        return TryAddAsync(tenantInfo);
    }
}