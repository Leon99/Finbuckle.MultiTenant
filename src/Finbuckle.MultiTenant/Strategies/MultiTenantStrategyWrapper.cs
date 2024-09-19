// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Logging;

namespace Finbuckle.MultiTenant.Strategies;

public class MultiTenantStrategyWrapper(IMultiTenantStrategy strategy, ILogger logger) : IMultiTenantStrategy
{
    public IMultiTenantStrategy Strategy { get; } = strategy ?? throw new ArgumentNullException(nameof(strategy));

    readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc />
    public async Task<string?> GetKeyAsync(object context)
    {
        ArgumentNullException.ThrowIfNull(context);

        string? key;

        try
        {
            key = await Strategy.GetKeyAsync(context);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            throw new MultiTenantException($"Exception in {Strategy.GetType()}.{nameof(GetKeyAsync)}.", e);
        }

        
        if(key is not null)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("GetIdentifierAsync: Found key: \"{Key}\"", key);
            }
        }
        else
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("GetIdentifierAsync: No key found");
            }
        }

        return key;
    }
}