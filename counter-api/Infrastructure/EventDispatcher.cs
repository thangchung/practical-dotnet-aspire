using CoffeeShop.MessageContracts;
using CoffeeShop.Shared.Domain;
using CounterApi.Domain.DomainEvents;

namespace CounterApi.Infrastructure;

public class EventDispatcher(IPublishEndpoint publisher) : INotificationHandler<EventWrapper>
{
	public virtual async Task Handle(EventWrapper @eventWrapper, CancellationToken cancellationToken)
	{
		switch (@eventWrapper.Event)
		{
			case BaristaOrdersPlacedDomainEvent @event:
				await publisher.Publish<BaristaOrderPlaced>(new {
					@event.OrderId,
					@event.ItemLines
				}, cancellationToken);
				break;
			case KitchenOrdersPlacedDomainEvent @event:
				await publisher.Publish<KitchenOrderPlaced>(new
				{
					@event.OrderId,
					@event.ItemLines
				}, cancellationToken);
				break;
		}
	}
}
