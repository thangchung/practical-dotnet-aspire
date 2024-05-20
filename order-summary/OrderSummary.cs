using CoffeeShop.MessageContracts;

using Marten.Events.Projections;

namespace CoffeeShop.OrderSummary;

public record Order(Guid Id)
{
	public Order Apply(BaristaOrderPlaced @event) =>
		this with { };

	public Order Apply(KitchenOrderPlaced @event) =>
		this with { };

	public Order Apply(BaristaOrderUpdated @event) =>
		this with { };

	public Order Apply(KitchenOrderUpdated @event) =>
		this with { };
}

public class OrderSummary
{
	public Guid Id { get; set; }
	public int NumberOfBaristaProcessed { get; set; }
	public int NumberOfKitchenProcessed { get; set; }
	public int NumberOfBaristaUpdated { get; set; }
	public int NumberOfKitchenUpdated { get; set; }
}

public class OrderSummaryProjection : MultiStreamProjection<OrderSummary, Guid>
{
	public OrderSummaryProjection()
	{
		Identity<BaristaOrderPlaced>(e => e.OrderId);
		Identity<KitchenOrderPlaced>(e => e.OrderId);
		Identity<BaristaOrderUpdated>(e => e.OrderId);
		Identity<KitchenOrderUpdated>(e => e.OrderId);
	}

	public void Apply(BaristaOrderPlaced @event, OrderSummary current)
	{
		current.NumberOfBaristaProcessed++;
	}

	public void Apply(KitchenOrderPlaced @event, OrderSummary current)
	{
		current.NumberOfKitchenProcessed++;
	}

	public void Apply(BaristaOrderUpdated @event, OrderSummary current)
	{
		current.NumberOfBaristaUpdated++;
	}

	public void Apply(KitchenOrderUpdated @event, OrderSummary current)
	{
		current.NumberOfKitchenUpdated++;
	}
}