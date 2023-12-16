using CounterApi.Domain.SharedKernel;

namespace CounterApi.Domain.DomainEvents;

public class OrderIn(Guid orderId, Guid itemLineId, ItemType itemType) : IDomainEvent
{
    public Guid OrderId { get; set; } = orderId;
    public Guid ItemLineId { get; set; } = itemLineId;
    public ItemType ItemType { get; set; } = itemType;
    public DateTime TimeIn { get; set; } = DateTime.UtcNow;
}

public class BaristaOrderIn(Guid orderId, Guid itemLineId, ItemType itemType) : OrderIn(orderId, itemLineId, itemType)
{
}

public class KitchenOrderIn(Guid orderId, Guid itemLineId, ItemType itemType) : OrderIn(orderId, itemLineId, itemType)
{
}