using MediatR;

namespace CounterApi.Domain.SharedKernel;

public interface IDomainEvent : INotification
{
}

public class EventWrapper(IDomainEvent @event) : INotification
{
    public IDomainEvent Event => @event;
}