using CoffeeShop.Shared.Helpers;

namespace CoffeeShop.Shared.Domain;

public interface IEntity
{
	public Guid Id { get; }
	public DateTime Created { get; }
	public DateTime? Updated { get; }
}

public interface IAggregateRoot : IEntity
{
	public HashSet<IDomainEvent> DomainEvents { get; }
}

public interface ITxRequest { }

public abstract class EntityRootBase(Guid? id) : EntityBase, IAggregateRoot
{
	public new Guid Id { get; } = id ?? GuidHelper.NewGuid();

	[JsonIgnore]
	public HashSet<IDomainEvent> DomainEvents { get; private set; } = [];

	public void AddDomainEvent(IDomainEvent eventItem)
	{
		DomainEvents.Add(eventItem);
	}

	public void RemoveDomainEvent(EventBase eventItem)
	{
		DomainEvents?.Remove(eventItem);
	}
}

public abstract class EntityBase : IEntity
{
	public Guid Id { get; private set; } = GuidHelper.NewGuid();
	public DateTime Created { get; } = DateTimeHelper.NewDateTime();
	public DateTime? Updated { get; protected set; }
}

/// <summary>
/// ref: https://github.com/dotnet/eShop/blob/main/src/Ordering.Domain/SeedWork/ValueObject.cs
/// </summary>
public abstract class ValueObject
{
	protected static bool EqualOperator(ValueObject left, ValueObject right)
	{
		if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
		{
			return false;
		}
		return ReferenceEquals(left, null) || left.Equals(right);
	}

	protected static bool NotEqualOperator(ValueObject left, ValueObject right)
	{
		return !(EqualOperator(left, right));
	}

	protected abstract IEnumerable<object> GetEqualityComponents();

	public override bool Equals(object obj)
	{
		if (obj == null || obj.GetType() != GetType())
		{
			return false;
		}

		var other = (ValueObject)obj;

		return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
	}

	public override int GetHashCode()
	{
		return GetEqualityComponents()
			.Select(x => x != null ? x.GetHashCode() : 0)
			.Aggregate((x, y) => x ^ y);
	}

	public ValueObject GetCopy()
	{
		return MemberwiseClone() as ValueObject;
	}
}

public static class AggregateRootExtensions
{
	public static async Task RelayAndPublishEvents(this IAggregateRoot aggregateRoot, IPublisher publisher, CancellationToken cancellationToken = default)
	{
		if (aggregateRoot.DomainEvents is not null)
		{
			var @events = new IDomainEvent[aggregateRoot.DomainEvents.Count];
			aggregateRoot.DomainEvents.CopyTo(@events);
			aggregateRoot.DomainEvents.Clear();

			foreach (var @event in @events)
			{
				await publisher.Publish(new EventWrapper(@event), cancellationToken);
			}
		}
	}
}
