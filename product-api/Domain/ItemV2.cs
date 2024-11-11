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

	public ItemV2(Guid id, decimal price, ItemType type) : this(id, price, type, null)
	{
	}

	public ItemV2(Guid id, decimal price, ItemType type, string? desc) : base(id)
	{
		Price = price;
		Type = type;
		Description = desc;
	}

	public decimal Price { get; private set; }
	public ItemType Type { get; private set; }
	public string? Description { get; private set; }

	[JsonIgnore]
	public Vector? Embedding { get; set; }
}

public class ItemV2Data : List<ItemV2>
{
	public ItemV2Data()
	{
		AddRange(
		[
			new(GuidHelper.NewGuid(), 4.5m, ItemType.CAPPUCCINO, "Rich espresso with steamed milk and frothy foam."),
			new(GuidHelper.NewGuid(), 3m, ItemType.COFFEE_BLACK, "Strong, pure coffee with no milk or sugar added."),
			new(GuidHelper.NewGuid(), 3m, ItemType.COFFEE_WITH_ROOM, "Black coffee with space left for milk or cream."),
			new(GuidHelper.NewGuid(), 3.5m, ItemType.ESPRESSO, "Intense, concentrated coffee shot with rich flavor."),
			new(GuidHelper.NewGuid(), 4.5m, ItemType.ESPRESSO_DOUBLE, "Two shots of rich, concentrated espresso coffee."),
			new(GuidHelper.NewGuid(), 4.5m, ItemType.LATTE, "Smooth espresso with steamed milk and light foam."),
			new(GuidHelper.NewGuid(), 2.5m, ItemType.CAKEPOP, "Mini cake on a stick, coated in sweet icing."),
			new(GuidHelper.NewGuid(), 2.5m, ItemType.CROISSANT, "Flaky, buttery pastry with a golden, crisp crust."),
			new(GuidHelper.NewGuid(), 2.5m, ItemType.MUFFIN, "Soft, moist baked treat, often with fruit or nuts."),
			new(GuidHelper.NewGuid(), 3m, ItemType.CROISSANT_CHOCOLATE, "Flaky croissant filled with rich, melted chocolate."),
			new(GuidHelper.NewGuid(), 5m, ItemType.CHICKEN_MEATBALLS, "Juicy, flavorful meatballs made from chicken."),
		]);
	}

	public ItemV2Data(IEnumerable<ItemV2> items)
	{
		AddRange(items);
	}
}
