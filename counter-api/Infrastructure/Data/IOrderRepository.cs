using CounterApi.Domain;

namespace CounterApi.Infrastructure.Data;

public class InMemDataStore
{
    public IDictionary<Guid, Order> Data { get; set; } = new Dictionary<Guid, Order>();
}

public class InMemOrderRepository(InMemDataStore store)
{
}