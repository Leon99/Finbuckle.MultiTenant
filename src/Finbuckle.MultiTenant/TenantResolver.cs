// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Events;
using Finbuckle.MultiTenant.Stores;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant;

/// <summary>
/// Resolves the current tenant.
/// </summary>
/// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
public class TenantResolver<TTenantInfo> : ITenantResolver<TTenantInfo>
    where TTenantInfo : class, ITenantInfo, new()
{
    readonly IOptionsMonitor<MultiTenantOptions> _options;
    readonly ILoggerFactory? _loggerFactory;

    public TenantResolver(IEnumerable<IMultiTenantStrategy> strategies,
        IEnumerable<IMultiTenantStore<TTenantInfo>> stores, IOptionsMonitor<MultiTenantOptions> options) :
        this(strategies, stores, options, null)
    {
    }

    public TenantResolver(IEnumerable<IMultiTenantStrategy> strategies,
        IEnumerable<IMultiTenantStore<TTenantInfo>> stores, IOptionsMonitor<MultiTenantOptions> options,
        ILoggerFactory? loggerFactory)
    {
        Stores = stores;
        _options = options;
        _loggerFactory = loggerFactory;

        Strategies = strategies.OrderByDescending(s => s.Priority);
    }

    /// <inheritdoc />
    public IEnumerable<IMultiTenantStrategy> Strategies { get; set; }

    /// <inheritdoc />
    public IEnumerable<IMultiTenantStore<TTenantInfo>> Stores { get; set; }

    /// <inheritdoc />
    public async Task<IMultiTenantContext<TTenantInfo>> ResolveAsync(object context)
    {
        var mtc = new MultiTenantContext<TTenantInfo>();

        string? identifier = null;
        foreach (var strategy in Strategies)
        {
            var wrappedStrategy = new MultiTenantStrategyWrapper(strategy,
                _loggerFactory?.CreateLogger(strategy.GetType()) ?? NullLogger.Instance);
            identifier = await wrappedStrategy.GetKeyAsync(context);

            if (_options.CurrentValue.IgnoredIdentifiers.Contains(identifier, StringComparer.OrdinalIgnoreCase))
            {
                (_loggerFactory?.CreateLogger(GetType()) ?? NullLogger.Instance).LogInformation(
                    "Ignored identifier: {Identifier}", identifier);
                identifier = null;
            }

            if (identifier is null)
                continue;

            foreach (var store in Stores)
            {
                var wrappedStore = new MultiTenantStoreWrapper<TTenantInfo>(store,
                    _loggerFactory?.CreateLogger(store.GetType()) ?? NullLogger.Instance);
                var tenantInfo = await wrappedStore.TryGetByKeyAsync(identifier);
                if (tenantInfo is null)
                    continue;

                await _options.CurrentValue.Events.OnTenantResolved(new TenantResolvedContext
                {
                    Context = context,
                    TenantInfo = tenantInfo,
                    StrategyType = strategy.GetType(),
                    StoreType = store.GetType()
                });

                mtc.Store = store;
                mtc.Strategy = strategy;
                mtc.TenantInfo = tenantInfo;
                return mtc;
            }
        }

        await _options.CurrentValue.Events.OnTenantNotResolved(new TenantNotResolvedContext
            { Context = context, Identifier = identifier });
        return mtc;
    }

    /// <inheritdoc />
    async Task<IMultiTenantContext> ITenantResolver.ResolveAsync(object context)
    {
        return await ResolveAsync(context);
    }
}