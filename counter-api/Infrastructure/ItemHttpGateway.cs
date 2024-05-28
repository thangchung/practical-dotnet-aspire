using CounterApi.Domain;
using CounterApi.Domain.Dtos;

namespace CounterApi.Infrastructure.Gateways;

public class ItemHttpGateway(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<ItemHttpGateway> logger) : IItemGateway
{
    public async Task<IEnumerable<ItemDto>> GetItemsByType(ItemType[] itemTypes)
    {
        logger.LogInformation("Start to call GetItemsByType in Product Api");

        var httpClient = httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(config.GetValue<string>("ProductApiUrl")!); //todo: need truly service discovery

        var httpResponseMessage = await httpClient.GetFromJsonAsync<List<ItemDto>>(config.GetValue("GetItemTypesApiRoute", "/api/v1/item-types"));
        logger.LogInformation("Can get {Count} items", httpResponseMessage?.Count);
        logger.LogInformation("JSON: {HttpResponseMessage}", JsonSerializer.Serialize(httpResponseMessage));

        return httpResponseMessage ?? [];
    }
}