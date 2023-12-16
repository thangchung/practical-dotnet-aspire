using CounterApi.Domain;
using CounterApi.Domain.Dtos;

namespace CounterApi.Infrastructure.Gateways;

public class ItemDaprGateway(IConfiguration config, ILogger<ItemDaprGateway> logger) : IItemGateway
{
    public async Task<IEnumerable<ItemDto>> GetItemsByType(ItemType[] itemTypes)
    {
        logger.LogInformation("Start to call GetItemsByIdsAsync in Product Api");

        var productAppName = config.GetValue("ProductCatalogAppDaprName", "product-api-dapr-http");
        logger.LogInformation("ProductCatalogAppDaprName: {0}", productAppName);

        // todo
        // var httpResponseMessage = await daprClient.InvokeMethodAsync<List<ItemDto>>(
        //     HttpMethod.Get,
        //     productAppName,
        //     "v1-get-item-types");

        // logger.LogInformation("Can get {Count} items", httpResponseMessage?.Count);
        // return httpResponseMessage ?? new List<ItemDto>();

        return new List<ItemDto>();
    }
}