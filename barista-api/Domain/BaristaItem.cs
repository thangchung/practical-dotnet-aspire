using System.Text.Json.Serialization;

using BaristaApi.Domain.DomainEvents;
using BaristaApi.Domain.SharedKernel;

namespace BaristaApi.Domain;

public enum ItemType
{
    // Beverages
    CAPPUCCINO,
    COFFEE_BLACK,
    COFFEE_WITH_ROOM,
    ESPRESSO,
    ESPRESSO_DOUBLE,
    LATTE
}

// public class BaristaItem(ItemType itemType, string itemName, DateTime timeIn)
// {
//     [JsonIgnore]
//     public HashSet<IDomainEvent> DomainEvents { get; private set; } = new HashSet<IDomainEvent>();
    
//     public Guid Id { get; set; } = Guid.NewGuid();
//     public ItemType ItemType { get; } = itemType;
//     public string ItemName { get; } = itemName;
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

//     public static BaristaItem From(ItemType itemType, string itemName, DateTime timeIn)
//     {
//         return new BaristaItem(itemType, itemName, timeIn);
//     }

//     public BaristaItem SetTimeUp(Guid orderId, Guid itemLineId, DateTime timeUp)
//     {
//         AddDomainEvent(new BaristaOrderUp(orderId, itemLineId, ItemName, ItemType, DateTime.UtcNow, "tc"));
//         TimeUp = timeUp;
//         return this;
//     }
// }