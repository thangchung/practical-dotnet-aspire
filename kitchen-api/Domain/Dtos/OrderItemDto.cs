namespace KitchenApi.Domain.Dtos;

public record OrderItemDto(Guid ItemLineId, ItemType ItemType);