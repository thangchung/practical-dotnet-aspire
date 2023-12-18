using System.Text.Json.Serialization;

using KitchenApi.Domain.DomainEvents;
using KitchenApi.Domain.SharedKernel;


namespace KitchenApi.Domain;

public enum ItemType
{
    // Food
    CAKEPOP,
    CROISSANT,
    MUFFIN,
    CROISSANT_CHOCOLATE
}

// public class KitchenOrder(Guid orderId, ItemType itemType, DateTime timeIn)
// {
//     [JsonIgnore]
//     public HashSet<IDomainEvent> DomainEvents { get; private set; } = new HashSet<IDomainEvent>();

//     public Guid Id { get; set; } = Guid.NewGuid();
//     public Guid OrderId { get; } = orderId;
//     public ItemType ItemType { get; } = itemType;
//     public string ItemName { get; } = itemType.ToString();
//     public DateTime TimeIn { get; } = timeIn;
//     public DateTime TimeUp { get; private set; }

//     public void AddDomainEvent(IDomainEvent eventItem)
//     {
//         DomainEvents ??= new HashSet<IDomainEvent>();
//         DomainEvents.Add(eventItem);
//     }

//     public void RemoveDomainEvent(IDomainEvent eventItem)
//     {
//         DomainEvents?.Remove(eventItem);
//     }

//     public KitchenOrder SetTimeUp(Guid itemLineId, DateTime timeUp)
//     {
//         AddDomainEvent(new KitchenOrderUp(OrderId, itemLineId, ItemType.ToString(), ItemType, DateTime.UtcNow, "tc"));
//         TimeUp = timeUp;
//         return this;
//     }

//     public static KitchenOrder From(Guid orderId, ItemType itemType, DateTime timeIn)
//     {
//         return new KitchenOrder(orderId, itemType, timeIn);
//     }
// }