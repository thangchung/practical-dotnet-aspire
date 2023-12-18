using CounterApi.Domain.SharedKernel;

namespace CounterApi.Domain.DomainEvents;

public class OrderUpdate : IDomainEvent
{
    public Guid OrderId { get; }
    public Guid ItemLineId { get; }
    public ItemType ItemType { get; }
    public OrderStatus OrderStatus { get; }
    public string? MadeBy { get; }

    public OrderUpdate(Guid orderId, Guid itemLineId, ItemType itemType, OrderStatus orderStatus)
    {
        OrderId = orderId;
        ItemLineId = itemLineId;
        ItemType = itemType;
        OrderStatus = orderStatus;
        MadeBy = null;
    }

    public OrderUpdate(Guid orderId, Guid itemLineId, ItemType itemType, OrderStatus orderStatus, string madeBy)
    {
        OrderId = orderId;
        ItemLineId = itemLineId;
        ItemType = itemType;
        OrderStatus = orderStatus;
        MadeBy = madeBy;
    }
}