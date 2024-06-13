using BaristaApi.Domain;

using CoffeeShop.MessageContracts;

using MassTransit;

namespace BaristaApi.IntegrationEvents.EventHandlers;

internal class BaristaOrderedConsumer(IPublishEndpoint publisher, ILogger<BaristaOrderedConsumer> logger)
	: IConsumer<BaristaOrderPlaced>
{
	public async Task Consume(ConsumeContext<BaristaOrderPlaced> context)
	{
		ArgumentNullException.ThrowIfNull(context);
		logger.LogInformation("Received an message {name}", nameof(context.Message));

		var message = context.Message;

		// toto: processing and persist it

		foreach (var item in message.ItemLines)
		{
			await Task.Delay(CalculateDelay(item.ItemType));
		}

		await publisher.Publish(new BaristaOrderUpdated { OrderId = message.OrderId, ItemLines = message.ItemLines });
	}

	private static TimeSpan CalculateDelay(ItemType itemType)
	{
		return itemType switch
		{
			ItemType.COFFEE_BLACK => TimeSpan.FromSeconds(5),
			ItemType.COFFEE_WITH_ROOM => TimeSpan.FromSeconds(5),
			ItemType.ESPRESSO => TimeSpan.FromSeconds(7),
			ItemType.ESPRESSO_DOUBLE => TimeSpan.FromSeconds(7),
			ItemType.CAPPUCCINO => TimeSpan.FromSeconds(10),
			_ => TimeSpan.FromSeconds(3)
		};
	}
}

internal class BaristaOrderedConsumerDefinition : ConsumerDefinition<BaristaOrderedConsumer>
{
	public BaristaOrderedConsumerDefinition()
	{
		// override the default endpoint name
		EndpointName = "barista-service";

		// limit the number of messages consumed concurrently
		// this applies to the consumer only, not the endpoint
		ConcurrentMessageLimit = 8;
	}

	protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
		IConsumerConfigurator<BaristaOrderedConsumer> consumerConfigurator)
	{
		// configure message retry with millisecond intervals
		endpointConfigurator.UseMessageRetry(r => r.Intervals(100, 200, 500, 800, 1000));

		// use the outbox to prevent duplicate events from being published
		endpointConfigurator.UseInMemoryOutbox();
	}
}