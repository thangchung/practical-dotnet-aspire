using CoffeeShop.MessageContracts;
using CoffeeShop.Shared.Helpers;

using Marten;

using MassTransit;

namespace CoffeeShop.OrderSummary.Consumers;

public class OrderConsumer(IDocumentSession documentSession, ILogger<OrderConsumer> logger) : IConsumer<BaristaOrderPlaced>, IConsumer<KitchenOrderPlaced>
{
	public async Task Consume(ConsumeContext<BaristaOrderPlaced> context)
	{
		logger.LogInformation("Consumer: {0} with orderId={1}", nameof(BaristaOrderPlaced), context.Message.OrderId);

		await documentSession.Events.WriteToAggregate<Order>(
			GuidHelper.NewGuid(), 
			stream => { stream.AppendOne(context.Message); });
	}

	public async Task Consume(ConsumeContext<KitchenOrderPlaced> context)
	{
		logger.LogInformation("Consumer: {0} with orderId={1}", nameof(KitchenOrderPlaced), context.Message.OrderId);

		await documentSession.Events.WriteToAggregate<Order>(
			GuidHelper.NewGuid(),
			stream => { stream.AppendOne(context.Message); });
	}
}
