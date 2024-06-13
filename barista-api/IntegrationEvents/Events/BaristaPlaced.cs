using BaristaApi.Domain.Dtos;

namespace CoffeeShop.MessageContracts;

public record BaristaOrderPlaced
{
    public Guid OrderId { get; init; }
    public List<OrderItemDto> ItemLines { get; init; } = [];
}