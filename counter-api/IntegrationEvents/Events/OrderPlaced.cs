using CounterApi.Domain.Dtos;

namespace CoffeeShop.MessageContracts;

public record BaristaOrderPlaced
{
    public Guid OrderId { get; init; }
    public List<OrderItemLineDto> ItemLines { get; init; } = [];
}

public record KitchenOrderPlaced
{
    public Guid OrderId { get; init; }
    public List<OrderItemLineDto> ItemLines { get; init; } = [];
}