namespace CoffeeShop.MessageContracts;

public enum ItemType
{
	// Beverages
	CAPPUCCINO,
	COFFEE_BLACK,
	COFFEE_WITH_ROOM,
	ESPRESSO,
	ESPRESSO_DOUBLE,
	LATTE,
	// Food
	CAKEPOP,
	CROISSANT,
	MUFFIN,
	CROISSANT_CHOCOLATE
}

public enum ItemStatus
{
	PLACED,
	IN_PROGRESS,
	FULFILLED
}

public record OrderItemLineDto(Guid ItemLineId, ItemType ItemType, ItemStatus ItemStatus);

public record BaristaOrderPlaced
{
	public Guid OrderId { get; init; }
	public List<OrderItemLineDto> ItemLines { get; init; } = [];
}

public record KitchenOrderPlaced
{
	public Guid OrderId { get; init; }
	public List<OrderItemLineDto> ItemLines { get; init; } = [];
}

public record BaristaOrderUpdated
{
	public Guid OrderId { get; init; }
	public List<OrderItemLineDto> ItemLines { get; init; } = [];
}

public record KitchenOrderUpdated
{
	public Guid OrderId { get; init; }
	public List<OrderItemLineDto> ItemLines { get; init; } = [];
}
