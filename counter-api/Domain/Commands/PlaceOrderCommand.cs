using CoffeeShop.Shared.Helpers;

using MediatR;

namespace CounterApi.Domain.Commands;

public class CommandItem
{
	public ItemType ItemType { get; set; }
}

public enum CommandType
{
	PLACE_ORDER
}

public class PlaceOrderCommand : IRequest<IResult>
{
	public Guid OrderId { get; set; } = GuidHelper.NewGuid();
	public CommandType CommandType { get; set; } = CommandType.PLACE_ORDER;
	public OrderSource OrderSource { get; set; } = OrderSource.COUNTER;
	public Location Location { get; set; } = Location.ATLANTA;
	public Guid LoyaltyMemberId { get; set; } = GuidHelper.NewGuid();
	public List<CommandItem> BaristaItems { get; set; } = [];
	public List<CommandItem> KitchenItems { get; set; } = [];
	public DateTime Timestamp { get; set; } = DateTimeHelper.NewDateTime();
}