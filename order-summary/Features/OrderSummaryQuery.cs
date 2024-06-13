using CoffeeShop.MessageContracts;
using CoffeeShop.Shared.Endpoint;

namespace CoffeeShop.OrderSummary.Features;

public class OrderSummaryEndpoint : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapGet("summary", (HttpContext context, IQuerySession querySession, Guid orderId) =>
			querySession.Json.WriteById<OrderSummaryQuery>(orderId, context)
		);
	}
}

public class OrderSummaryQuery
{
	public Guid Id { get; set; }
	public int NumberOfBaristaProcessed { get; set; }
	public int NumberOfKitchenProcessed { get; set; }
	public int NumberOfBaristaUpdated { get; set; }
	public int NumberOfKitchenUpdated { get; set; }
}

public class OrderSummaryProjection : MultiStreamProjection<OrderSummaryQuery, Guid>
{
	public OrderSummaryProjection()
	{
		Identity<BaristaOrderPlaced>(e => e.OrderId);
		Identity<KitchenOrderPlaced>(e => e.OrderId);
		Identity<BaristaOrderUpdated>(e => e.OrderId);
		Identity<KitchenOrderUpdated>(e => e.OrderId);
	}

	public void Apply(BaristaOrderPlaced @event, OrderSummaryQuery current)
	{
		current.NumberOfBaristaProcessed++;
	}

	public void Apply(KitchenOrderPlaced @event, OrderSummaryQuery current)
	{
		current.NumberOfKitchenProcessed++;
	}

	public void Apply(BaristaOrderUpdated @event, OrderSummaryQuery current)
	{
		current.NumberOfBaristaUpdated++;
	}

	public void Apply(KitchenOrderUpdated @event, OrderSummaryQuery current)
	{
		current.NumberOfKitchenUpdated++;
	}
}
