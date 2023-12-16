using KitchenApi.Domain.Dtos;

namespace KitchenApi.Domain.Messages;

public record KitchenOrderPlaced
{
    public Guid OrderId { get; init; }
    public List<OrderItemDto> ItemLines { get; init; } = new();
}

public record KitchenOrderUpdated
{
    public Guid OrderId { get; init; }
    public List<OrderItemDto> ItemLines { get; init; } = new();
}