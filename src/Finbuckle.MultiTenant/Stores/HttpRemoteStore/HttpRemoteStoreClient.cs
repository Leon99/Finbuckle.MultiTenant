// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Text.Json;
using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Stores.HttpRemoteStore;

public class HttpRemoteStoreClient<TTenantInfo>(IHttpClientFactory clientFactory)
    where TTenantInfo : class, ITenantInfo, new()
{
    private readonly IHttpClientFactory _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));

    public async Task<TTenantInfo?> TryGetByIdentifierAsync(string endpointTemplate, string identifier)
    {
        var client = _clientFactory.CreateClient(typeof(HttpRemoteStoreClient<TTenantInfo>).FullName!);
        var uri = endpointTemplate.Replace(HttpRemoteStore<TTenantInfo>.DefaultEndpointTemplateIdentifierToken, identifier);
        var response = await client.GetAsync(uri);

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TTenantInfo>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        return result;
    }
}