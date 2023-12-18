namespace CounterApi.Domain.Dtos;

public record ItemDto(string Name, decimal Price, ItemType ItemType, string Image);

public record ItemTypeDto(ItemType Type, string Name);

public record OrderItemLineDto(Guid ItemLineId, ItemType ItemType, ItemStatus ItemStatus);

public class OrderDto
{
    public Guid Id { get; set; }
    public OrderSource OrderSource { get; set; }
    public Guid LoyaltyMemberId { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public Location Location { get; set; }
    public List<OrderItemLineDto> ItemLines { get; set; } = new();
}
