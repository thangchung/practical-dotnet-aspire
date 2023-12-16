using BaristaApi.Domain.Dtos;

namespace BaristaApi.Domain.Messages;

public record BaristaOrderPlaced
{
    public Guid OrderId { get; init; }
    public List<OrderItemDto> ItemLines { get; init; } = new();
}

public record BaristaOrderUpdated
{
    public Guid OrderId { get; init; }
    public List<OrderItemDto> ItemLines { get; init; } = new();
}