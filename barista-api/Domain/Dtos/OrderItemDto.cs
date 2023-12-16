namespace BaristaApi.Domain.Dtos;

public record OrderItemDto(Guid ItemLineId, ItemType ItemType);