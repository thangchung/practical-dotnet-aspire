using CounterApi.Domain.Dtos;

namespace CoffeeShop.MessageContracts;

public record BaristaOrderUpdated
{
	public Guid OrderId { get; init; }
	public List<OrderItemLineDto> ItemLines { get; init; } = new();
}

public record KitchenOrderUpdated
{
	public Guid OrderId { get; init; }
	public List<OrderItemLineDto> ItemLines { get; init; } = new();
}