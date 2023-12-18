using KitchenApi.Domain.SharedKernel;

namespace KitchenApi.Domain.DomainEvents;

// public class KitchenOrderUp(Guid orderId, Guid itemLineId, string name, ItemType itemType, DateTime timeUp, string madeBy) : IDomainEvent
// {
//     // OrderIn info
//     public Guid OrderId { get; } = orderId;
//     public Guid ItemLineId { get; } = itemLineId;
//     public string Name { get; } = name;
//     public ItemType ItemType { get; } = itemType;
//     public DateTime TimeIn { get; } = DateTime.UtcNow;
//     public string MadeBy { get; } = madeBy;
//     public DateTime TimeUp { get; } = timeUp;
// }