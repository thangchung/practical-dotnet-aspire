using CoffeeShop.MessageContracts;
using CoffeeShop.Shared.Domain;
using CounterApi.Domain.DomainEvents;
using MassTransit;

using MediatR;

namespace CounterApi.Infrastructure;

public class EventDispatcher(IPublishEndpoint publisher) : INotificationHandler<EventWrapper>
{
	private readonly IPublishEndpoint _publisher = publisher;

	public virtual async Task Handle(EventWrapper @eventWrapper, CancellationToken cancellationToken)
	{
		switch (@eventWrapper.Event)
		{
			case BaristaOrdersPlacedDomainEvent @event:
				await _publisher.Publish<BaristaOrderPlaced>(new {
					@event.OrderId,
					@event.ItemLines
				}, cancellationToken);
				break;
			case KitchenOrdersPlacedDomainEvent @event:
				await _publisher.Publish<KitchenOrderPlaced>(new
				{
					@event.OrderId,
					@event.ItemLines
				}, cancellationToken);
				break;
		}
	}
}
