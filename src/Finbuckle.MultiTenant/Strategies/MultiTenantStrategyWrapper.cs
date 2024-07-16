// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Logging;

namespace Finbuckle.MultiTenant.Strategies;

public class MultiTenantStrategyWrapper(IMultiTenantStrategy strategy, ILogger logger) : IMultiTenantStrategy
{
    public IMultiTenantStrategy Strategy { get; } = strategy ?? throw new ArgumentNullException(nameof(strategy));

    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<string?> GetIdentifierAsync(object context)
    {
        ArgumentNullException.ThrowIfNull(context);

        string? identifier = null;

        try
        {
            identifier = await Strategy.GetIdentifierAsync(context);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception in GetIdentifierAsync");
            throw new MultiTenantException($"Exception in {Strategy.GetType()}.GetIdentifierAsync.", e);
        }

        if(identifier != null)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("GetIdentifierAsync: Found identifier: \"{Identifier}\"", identifier);
            }
        }
        else
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("GetIdentifierAsync: No identifier found");
            }
        }

        return identifier;
    }
}