using System.Diagnostics;

using CoffeeShop.MessageContracts;
using CoffeeShop.OrderSummary.Models;
using CoffeeShop.Shared.Helpers;

namespace CoffeeShop.OrderSummary.Consumers;

public class OrderConsumer(IDocumentSession documentSession, ILogger<OrderConsumer> logger) : IConsumer<BaristaOrderPlaced>, IConsumer<KitchenOrderPlaced>
{
	public async Task Consume(ConsumeContext<BaristaOrderPlaced> context)
	{
		logger.LogInformation("Consumer: {0} with orderId={1}", nameof(BaristaOrderPlaced), context.Message.OrderId);

		using var activity = new ActivitySource("masstransit").StartActivity($"Consumer: {nameof(BaristaOrderPlaced)} with orderId={context.Message.OrderId}");
		try
		{
			await documentSession.Events.WriteToAggregate<Order>(
			GuidHelper.NewGuid(),
			stream => { stream.AppendOne(context.Message); });
		}
		catch (Exception ex)
		{
			activity?.AddTag("exception.message", ex.Message);
			activity?.AddTag("exception.stacktrace", ex.ToString());
			activity?.AddTag("exception.type", ex.GetType().FullName);
			activity?.SetStatus(ActivityStatusCode.Error);
			throw;
		}
	}

	public async Task Consume(ConsumeContext<KitchenOrderPlaced> context)
	{
		logger.LogInformation("Consumer: {0} with orderId={1}", nameof(KitchenOrderPlaced), context.Message.OrderId);

		using var activity = new ActivitySource("masstransit").StartActivity($"Consumer: {nameof(BaristaOrderPlaced)} with orderId={context.Message.OrderId}");
		try
		{
			await documentSession.Events.WriteToAggregate<Order>(
			GuidHelper.NewGuid(),
			stream => { stream.AppendOne(context.Message); });
		}
		catch (Exception ex)
		{
			activity?.AddTag("exception.message", ex.Message);
			activity?.AddTag("exception.stacktrace", ex.ToString());
			activity?.AddTag("exception.type", ex.GetType().FullName);
			activity?.SetStatus(ActivityStatusCode.Error);
			throw;
		}
	}
}
