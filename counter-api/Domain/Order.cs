using CoffeeShop.Shared.Domain;

using CounterApi.Domain.Commands;
using CounterApi.Domain.DomainEvents;
using CounterApi.Domain.Dtos;

namespace CounterApi.Domain;

public class Order(Guid id, OrderSource orderSource, Guid loyaltyMemberId, OrderStatus orderStatus, Location location)
	: EntityRootBase(id)
{
	public OrderSource OrderSource { get; set; } = orderSource;
	public Guid LoyaltyMemberId { get; set; } = loyaltyMemberId;
	public OrderStatus OrderStatus { get; set; } = orderStatus;
	public Location Location { get; set; } = location;
	public List<ItemLine> ItemLines { get; set; } = [];

	public static async Task<Order> From(PlaceOrderCommand placeOrderCommand, IItemGateway itemGateway)
	{
		var order = new Order(placeOrderCommand.OrderId, placeOrderCommand.OrderSource, placeOrderCommand.LoyaltyMemberId, OrderStatus.IN_PROGRESS, placeOrderCommand.Location);

		if (placeOrderCommand.BaristaItems.Count != 0)
		{
			var itemTypes = placeOrderCommand.BaristaItems.Select(x => x.ItemType);
			var items = await itemGateway.GetItemsByType(itemTypes.ToArray());
			foreach (var baristaItem in placeOrderCommand.BaristaItems)
			{
				var item = items.FirstOrDefault(x => x.ItemType == baristaItem.ItemType);
				var itemLine = new ItemLine(baristaItem.ItemType, item?.ItemType.ToString()!, (decimal)item?.Price!, ItemStatus.IN_PROGRESS, true);

				// order.AddDomainEvent(new OrderUpdate(order.Id, itemLine.Id, itemLine.ItemType, OrderStatus.IN_PROGRESS));
				order.AddDomainEvent(new BaristaOrderIn(order.Id, itemLine.Id, itemLine.ItemType));

				order.ItemLines.Add(itemLine);
			}
		}

		if (placeOrderCommand.KitchenItems.Count != 0)
		{
			var itemTypes = placeOrderCommand.KitchenItems.Select(x => x.ItemType);
			var items = await itemGateway.GetItemsByType(itemTypes.ToArray());
			foreach (var kitchenItem in placeOrderCommand.KitchenItems)
			{
				var item = items.FirstOrDefault(x => x.ItemType == kitchenItem.ItemType);
				var itemLine = new ItemLine(kitchenItem.ItemType, item?.ItemType.ToString()!, (decimal)item?.Price!, ItemStatus.IN_PROGRESS, false);

				// order.AddDomainEvent(new OrderUpdate(order.Id, itemLine.Id, itemLine.ItemType, OrderStatus.IN_PROGRESS));
				order.AddDomainEvent(new KitchenOrderIn(order.Id, itemLine.Id, itemLine.ItemType));

				order.ItemLines.Add(itemLine);
			}
		}

		return order;
	}

	public Order Apply(OrderUp orderUp)
	{
		if (ItemLines.Count == 0) return this;

		var item = ItemLines.FirstOrDefault(i => i.Id == orderUp.ItemLineId);

		if (item is not null)
		{
			item.ItemStatus = ItemStatus.FULFILLED;
			// AddDomainEvent(new OrderUpdate(Id, item.Id, item.ItemType, OrderStatus.FULFILLED, orderUp.MadeBy));
		}

		// if there are both barista and kitchen items is fulfilled then checking status and change order to Fulfilled
		if (ItemLines.All(i => i.ItemStatus == ItemStatus.FULFILLED))
		{
			OrderStatus = OrderStatus.FULFILLED;
		}
		return this;
	}

	public Order DomainEventAggregation()
	{
		var baristaEvents = new BaristaOrdersPlacedDomainEvent();
		var kitchenEvents = new KitchenOrdersPlacedDomainEvent();
		foreach (var @event in DomainEvents)
		{
			switch (@event)
			{
				case BaristaOrderIn baristaOrderInEvent:
					baristaEvents.OrderId ??= baristaOrderInEvent.OrderId;
					baristaEvents.ItemLines.Add(
						new OrderItemLineDto(
							baristaOrderInEvent.ItemLineId,
							baristaOrderInEvent.ItemType,
							ItemStatus.IN_PROGRESS));
					break;
				case KitchenOrderIn kitchenOrderInEvent:
					kitchenEvents.OrderId ??= kitchenOrderInEvent.OrderId;
					kitchenEvents.ItemLines.Add(
						new OrderItemLineDto(
							kitchenOrderInEvent.ItemLineId,
							kitchenOrderInEvent.ItemType,
							ItemStatus.IN_PROGRESS));
					break;
			}
		}

		DomainEvents.Clear();
		DomainEvents.Add(baristaEvents);
		DomainEvents.Add(kitchenEvents);

		return this;
	}

	public static OrderDto ToDto(Order order)
	{
		var dto = new OrderDto
		{
			Id = order.Id,
			OrderStatus = order.OrderStatus,
			Location = order.Location,
			OrderSource = order.OrderSource,
			LoyaltyMemberId = order.LoyaltyMemberId
		};

		foreach (var item in order.ItemLines)
		{
			dto.ItemLines.Add(new OrderItemLineDto(item.Id, item.ItemType, item.ItemStatus));
		}

		return dto;
	}

	public static async Task<Order> FromDto(OrderDto dto, IItemGateway itemGateway)
	{
		var order = new Order(dto.Id, dto.OrderSource, dto.LoyaltyMemberId, OrderStatus.IN_PROGRESS, dto.Location);

		var itemTypes = dto.ItemLines.Select(x => x.ItemType);
		var items = await itemGateway.GetItemsByType(itemTypes.ToArray());

		foreach (var itemLineDto in dto.ItemLines)
		{
			var item = items.FirstOrDefault(x => x.ItemType == itemLineDto.ItemType);
			var itemLine = new ItemLine(itemLineDto.ItemLineId, itemLineDto.ItemType, item?.ItemType.ToString()!, (decimal)item?.Price!, ItemStatus.IN_PROGRESS, true);
			order.ItemLines.Add(itemLine);
		}

		return order;
	}
}

public class ItemLine(Guid id, ItemType itemType, string name, decimal price, ItemStatus itemStatus, bool isBarista)
{
	public Guid Id { get; set; } = id;
	public ItemType ItemType { get; set; } = itemType;
	public string Name { get; set; } = name;
	public decimal Price { get; set; } = price;
	public ItemStatus ItemStatus { get; set; } = itemStatus;
	public bool IsBaristaOrder { get; set; } = isBarista;

	public ItemLine(ItemType itemType, string name, decimal price, ItemStatus itemStatus, bool isBarista)
		: this(Guid.NewGuid(), itemType, name, price, itemStatus, isBarista)
	{
	}
}