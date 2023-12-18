using CounterApi.Domain.SharedKernel;

namespace CounterApi.Domain.DomainEvents;

public class OrderUp(Guid itemLineId) : IDomainEvent
{
    public Guid ItemLineId => itemLineId;
}

public class BaristaOrderUp(Guid itemLineId) : OrderUp(itemLineId)
{
}

public class KitchenOrderUp(Guid itemLineId) : OrderUp(itemLineId)
{
}