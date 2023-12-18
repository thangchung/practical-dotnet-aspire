namespace ProductApi.Domain;

public enum ItemType
{
    // Beverages
    CAPPUCCINO,
    COFFEE_BLACK,
    COFFEE_WITH_ROOM,
    ESPRESSO,
    ESPRESSO_DOUBLE,
    LATTE,
    // Food
    CAKEPOP,
    CROISSANT,
    MUFFIN,
    CROISSANT_CHOCOLATE
}

public class Item(decimal price, ItemType type)
{
    public decimal Price { get; private set; } = price;
    public ItemType Type { get; private set; } = type;

    #region beverages


    public static Item Cappuccino()
    {
        return new Item(4.5m, ItemType.CAPPUCCINO);
    }

    public static Item CoffeeBlack()
    {
        return new Item(3m, ItemType.COFFEE_BLACK);
    }

    public static Item CoffeeWithRoom()
    {
        return new Item(3m, ItemType.COFFEE_WITH_ROOM);
    }

    public static Item Espresso()
    {
        return new Item(3.5m, ItemType.ESPRESSO);
    }

    public static Item EspressoDouble()
    {
        return new Item(4.5m, ItemType.ESPRESSO_DOUBLE);
    }

    public static Item Latte()
    {
        return new Item(4.5m, ItemType.LATTE);
    }

    #endregion

    #region food

    public static Item CakePop()
    {
        return new Item(2.5m, ItemType.CAKEPOP);
    }

    public static Item Croissant()
    {
        return new Item(3.25m, ItemType.CROISSANT);
    }

    public static Item Muffin()
    {
        return new Item(3.0m, ItemType.MUFFIN);
    }

    public static Item CroissantChocolate()
    {
        return new Item(3.5m, ItemType.CROISSANT);
    }

    public static Item GetItem(ItemType type)
    {
        return type switch
        {
            ItemType.CAPPUCCINO => Cappuccino(),
            ItemType.COFFEE_BLACK => CoffeeBlack(),
            ItemType.COFFEE_WITH_ROOM => CoffeeWithRoom(),
            ItemType.ESPRESSO => Espresso(),
            ItemType.ESPRESSO_DOUBLE => EspressoDouble(),
            ItemType.LATTE => Latte(),
            ItemType.CAKEPOP => CakePop(),
            ItemType.CROISSANT => Croissant(),
            ItemType.MUFFIN => Muffin(),
            ItemType.CROISSANT_CHOCOLATE => CroissantChocolate(),
            _ => Cappuccino(),
        };
    }

    #endregion

    public override string ToString()
    {
        return $"{Type}-{Price}";
    }
}