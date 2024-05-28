using CoffeeShop.MessageContracts;

namespace CoffeeShop.OrderSummary.Models;

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
