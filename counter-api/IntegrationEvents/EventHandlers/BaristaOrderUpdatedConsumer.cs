using CoffeeShop.MessageContracts;

namespace CounterApi.IntegrationEvents.EventHandlers;

internal class BaristaOrderUpdatedConsumer(IPublishEndpoint publisher, ILogger<BaristaOrderUpdatedConsumer> logger) 
    : IConsumer<BaristaOrderUpdated>
{
    public async Task Consume(ConsumeContext<BaristaOrderUpdated> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        logger.LogInformation("Received an message {name}", nameof(context.Message));

        // todo
        // var message = context.Message;
        // var spec = new GetOrderByIdWithLineItemSpec(message.OrderId);
        // var order = await _orderRepository.FindOneAsync(spec);
        
        // var orderUpdated = order.Apply(
        //     new OrderUp(
        //         message.OrderId, 
        //         message.ItemLineId, 
        //         message.ItemType.ToString(), 
        //         message.ItemType,
        //         message.TimeUp,
        //         message.MadeBy));
        
        // await _orderRepository.EditAsync(orderUpdated);

        // await order.RelayAndPublishEvents(_publisher);

        await Task.CompletedTask;
    }
}

internal class BaristaOrderUpdatedConsumerDefinition : ConsumerDefinition<BaristaOrderUpdatedConsumer>
{
    public BaristaOrderUpdatedConsumerDefinition()
    {
        // override the default endpoint name
        EndpointName = "order-updated";

        // limit the number of messages consumed concurrently
        // this applies to the consumer only, not the endpoint
        ConcurrentMessageLimit = 1;
    }

    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<BaristaOrderUpdatedConsumer> consumerConfigurator)
    {
        // configure message retry with millisecond intervals
        endpointConfigurator.UseMessageRetry(r => r.Intervals(100, 200, 500, 800, 1000));

        // use the outbox to prevent duplicate events from being published
        endpointConfigurator.UseInMemoryOutbox();
    }
}