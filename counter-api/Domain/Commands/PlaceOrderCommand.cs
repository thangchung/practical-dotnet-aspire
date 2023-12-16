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
    public Guid OrderId { get; set; }
    public CommandType CommandType { get; set; } = CommandType.PLACE_ORDER;
    public OrderSource OrderSource { get; set; }
    public Location Location { get; set; }
    public Guid LoyaltyMemberId { get; set; }
    public List<CommandItem> BaristaItems { get; set; } = new();
    public List<CommandItem> KitchenItems { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}