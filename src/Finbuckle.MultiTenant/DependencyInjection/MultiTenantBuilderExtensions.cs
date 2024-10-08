// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Internal;
using Finbuckle.MultiTenant.Stores.ConfigurationStore;
using Finbuckle.MultiTenant.Stores.DistributedCacheStore;
using Finbuckle.MultiTenant.Stores.HttpRemoteStore;
using Finbuckle.MultiTenant.Stores.InMemoryStore;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Finbuckle.MultiTenant;

/// <summary>
/// Provides builder methods for Finbuckle.MultiTenant services and configuration.
/// </summary>
public static class MultiTenantBuilderExtensions
{
    /// <summary>
    /// Adds a DistributedCacheStore to the application.
    /// </summary>
    public static MultiTenantBuilder<TTenantInfo> WithDistributedCacheStore<TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder)
        where TTenantInfo : class, ITenantInfo, new()
        => builder.WithDistributedCacheStore(TimeSpan.MaxValue);


    /// <summary>
    /// Adds a DistributedCacheStore to the application.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="slidingExpiration">The timespan for a cache entry's sliding expiration.</param>
    public static MultiTenantBuilder<TTenantInfo> WithDistributedCacheStore<TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder, TimeSpan? slidingExpiration)
        where TTenantInfo : class, ITenantInfo, new()
    {
        var storeParams = slidingExpiration is null ? new object[] { Constants.TenantToken } : new object[] { Constants.TenantToken, slidingExpiration };

        return builder.WithStore<DistributedCacheStore<TTenantInfo>>(ServiceLifetime.Transient, storeParams);
    }

    /// <summary>
    /// Adds a HttpRemoteStore to the application.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="endpointTemplate">The endpoint URI template.</param>
    public static MultiTenantBuilder<TTenantInfo> WithHttpRemoteStore<TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder, string endpointTemplate)
        where TTenantInfo : class, ITenantInfo, new()
        => builder.WithHttpRemoteStore(endpointTemplate, null);

    /// <summary>
    /// Adds a HttpRemoteStore to the application.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="endpointTemplate">The endpoint URI template.</param>
    /// <param name="clientConfig">An action to configure the underlying HttpClient.</param>
    public static MultiTenantBuilder<TTenantInfo> WithHttpRemoteStore<TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder,
        string endpointTemplate,
        Action<IHttpClientBuilder>? clientConfig) where TTenantInfo : class, ITenantInfo, new()
    {
        var httpClientBuilder = builder.Services.AddHttpClient(typeof(HttpRemoteStoreClient<TTenantInfo>).FullName!);
        clientConfig?.Invoke(httpClientBuilder);

        builder.Services.TryAddSingleton<HttpRemoteStoreClient<TTenantInfo>>();

        return builder.WithStore<HttpRemoteStore<TTenantInfo>>(ServiceLifetime.Singleton, endpointTemplate);
    }

    /// <summary>
    /// Adds a ConfigurationStore to the application. Uses the default IConfiguration and section "Finbuckle:MultiTenant:Stores:ConfigurationStore".
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    public static MultiTenantBuilder<TTenantInfo> WithConfigurationStore<TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder)
        where TTenantInfo : class, ITenantInfo, new()
        => builder.WithStore<ConfigurationStore<TTenantInfo>>(ServiceLifetime.Singleton);

    /// <summary>
    /// Adds a ConfigurationStore to the application.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="configuration">The IConfiguration to load the section from.</param>
    /// <param name="sectionName">The configuration section to load.</param>
    public static MultiTenantBuilder<TTenantInfo> WithConfigurationStore<TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder,
        IConfiguration configuration,
        string sectionName)
        where TTenantInfo : class, ITenantInfo, new()
        => builder.WithStore<ConfigurationStore<TTenantInfo>>(ServiceLifetime.Singleton, configuration, sectionName);

    /// <summary>
    /// Adds an empty InMemoryStore to the application.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    public static MultiTenantBuilder<TTenantInfo> WithInMemoryStore<TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder)
        where TTenantInfo : class, ITenantInfo, new()
    // ReSharper disable once RedundantTypeArgumentsOfMethod
        => builder.WithInMemoryStore<TTenantInfo>(_ => {});

    /// <summary>
    /// Adds and configures InMemoryStore to the application using the provided action.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="config">An action for configuring the store.</param>
    public static MultiTenantBuilder<TTenantInfo> WithInMemoryStore<TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder,
        Action<InMemoryStoreOptions<TTenantInfo>> config)
        where TTenantInfo : class, ITenantInfo, new()
    {
        ArgumentNullException.ThrowIfNull(config);

        // ReSharper disable once RedundantTypeArgumentsOfMethod
        builder.Services.Configure<InMemoryStoreOptions<TTenantInfo>>(config);

        return builder.WithStore<InMemoryStore<TTenantInfo>>(ServiceLifetime.Singleton);
    }

    /// <summary>
    /// Adds and configures a StaticStrategy to the application.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="key">The tenant identifier to use for all tenant resolution.</param>
    public static MultiTenantBuilder<TTenantInfo> WithStaticStrategy<TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder,
        string key)
        where TTenantInfo : class, ITenantInfo, new()
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentNullException(nameof(key), "Invalid value");
        }

        return builder.WithStrategy<StaticStrategy>(ServiceLifetime.Singleton, new object[] { key });
    }

    /// <summary>
    /// Adds and configures a DelegateStrategy to the application.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="doStrategy">The delegate implementing the strategy.</param>
    public static MultiTenantBuilder<TTenantInfo> WithDelegateStrategy<TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder,
        Func<object, Task<string?>> doStrategy)
        where TTenantInfo : class, ITenantInfo, new()
    {
        ArgumentNullException.ThrowIfNull(doStrategy);

        return builder.WithStrategy<DelegateStrategy>(ServiceLifetime.Singleton, [doStrategy]);
    }
}