using System.Text.Json.Serialization;

using CoffeeShop.Shared.Domain;
using CoffeeShop.Shared.Helpers;

using Pgvector;

namespace ProductApi.Domain;

public class ItemV2 : EntityRootBase
{
	private ItemV2() : base(GuidHelper.NewGuid())
	{
	}

	public ItemV2(Guid id, decimal price, ItemType type) : base(id)
	{
		Price = price;
		Type = type;
	}

	public decimal Price { get; private set; }
	public ItemType Type { get; private set; }

	[JsonIgnore]
	public Vector? Embedding { get; set; }
}

public class ItemV2Data : List<ItemV2>
{
	public ItemV2Data()
	{
		AddRange(
		[
			new(Guid.NewGuid(), 4.5m, ItemType.CAPPUCCINO),
			new(Guid.NewGuid(), 3m, ItemType.COFFEE_BLACK),
			new(Guid.NewGuid(), 3m, ItemType.COFFEE_WITH_ROOM),
			new(Guid.NewGuid(), 3.5m, ItemType.ESPRESSO),
			new(Guid.NewGuid(), 4.5m, ItemType.ESPRESSO_DOUBLE),
			new(Guid.NewGuid(), 4.5m, ItemType.LATTE),
			new(Guid.NewGuid(), 2.5m, ItemType.CAKEPOP),
			new(Guid.NewGuid(), 2.5m, ItemType.CROISSANT),
			new(Guid.NewGuid(), 2.5m, ItemType.MUFFIN),
			new(Guid.NewGuid(), 3m, ItemType.CROISSANT_CHOCOLATE)
		]);
	}

	public ItemV2Data(IEnumerable<ItemV2> items)
	{
		AddRange(items);
	}
}
