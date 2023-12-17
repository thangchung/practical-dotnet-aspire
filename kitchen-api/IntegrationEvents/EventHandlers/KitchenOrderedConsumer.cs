using KitchenApi.Domain;
using CoffeeShop.MessageContracts;
using MassTransit;

namespace KitchenApi.IntegrationEvents.EventHandlers;

public class KitchenOrderedConsumer(IPublishEndpoint publisher, ILogger<KitchenOrderedConsumer> logger) 
    : IConsumer<KitchenOrderPlaced>
{
    public async Task Consume(ConsumeContext<KitchenOrderPlaced> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        logger.LogInformation("Received an message {name}", nameof(context.Message));

        var message = context.Message;

        foreach(var item in message.ItemLines) {
            await Task.Delay(CalculateDelay(item.ItemType));
        }

        await publisher.Publish(new KitchenOrderUpdated{OrderId = message.OrderId, ItemLines = message.ItemLines});
    }

    private static TimeSpan CalculateDelay(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.CROISSANT => TimeSpan.FromSeconds(7),
            ItemType.CROISSANT_CHOCOLATE => TimeSpan.FromSeconds(7),
            ItemType.CAKEPOP => TimeSpan.FromSeconds(5),
            ItemType.MUFFIN => TimeSpan.FromSeconds(7),
            _ => TimeSpan.FromSeconds(3)
        };
    }
}

internal class KitchenOrderedConsumerDefinition : ConsumerDefinition<KitchenOrderedConsumer>
{
    public KitchenOrderedConsumerDefinition()
    {
        // override the default endpoint name
        EndpointName = "kitchen-service";

        // limit the number of messages consumed concurrently
        // this applies to the consumer only, not the endpoint
        ConcurrentMessageLimit = 8;
    }

    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<KitchenOrderedConsumer> consumerConfigurator)
    {
        // configure message retry with millisecond intervals
        endpointConfigurator.UseMessageRetry(r => r.Intervals(100, 200, 500, 800, 1000));

        // use the outbox to prevent duplicate events from being published
        endpointConfigurator.UseInMemoryOutbox();
    }
}