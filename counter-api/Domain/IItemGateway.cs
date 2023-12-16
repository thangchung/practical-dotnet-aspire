using CounterApi.Domain.Dtos;

namespace CounterApi.Domain;

public interface IItemGateway
{
    Task<IEnumerable<ItemDto>> GetItemsByType(ItemType[] itemTypes);
}