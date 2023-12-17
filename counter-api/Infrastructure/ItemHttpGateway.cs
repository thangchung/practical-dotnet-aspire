using System.Text.Json;

using CounterApi.Domain;
using CounterApi.Domain.Dtos;

namespace CounterApi.Infrastructure.Gateways;

public class ItemHttpGateway(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<ItemHttpGateway> logger) : IItemGateway
{
    public async Task<IEnumerable<ItemDto>> GetItemsByType(ItemType[] itemTypes)
    {
        logger.LogInformation("Start to call GetItemsByIdsAsync in Product Api");

        var productAppName = config.GetValue("ProductCatalogAppDaprName", "product-api-dapr-http");
        logger.LogInformation("ProductCatalogAppDaprName: {0}", productAppName);

        var httpClient = httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(config.GetValue("ProductApiUri", "http://localhost:5001")!);

        var httpResponseMessage = await httpClient.GetFromJsonAsync<List<ItemDto>>(config.GetValue("GetItemTypesApiRoute", "/v1/api/item-types"));
        logger.LogInformation("Can get {Count} items", httpResponseMessage?.Count);
        logger.LogInformation("JSON: {content}", JsonSerializer.Serialize(httpResponseMessage));

        return httpResponseMessage ?? [];
    }
}