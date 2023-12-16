using BaristaApi.Domain.SharedKernel;

namespace BaristaApi.Domain.DomainEvents;

// OrderIn info
// public record BaristaOrderUp(Guid OrderId, Guid ItemLineId, string Name, ItemType ItemType, DateTime TimeIn, string MadeBy, DateTime TimeUp) : IDomainEvent
// {
//     public BaristaOrderUp(Guid orderId, Guid itemLineId, string name, ItemType itemType, DateTime timeUp, string madeBy) : this(orderId, itemLineId, name, itemType, DateTime.UtcNow, madeBy, timeUp)
//     {
//     }
// }