namespace CoffeeShop.Shared.Helpers;

[DebuggerStepThrough]
public static class GuidHelper
{
	public static Guid NewGuid()
	{
		return NewId.NextGuid();
	}

	public static bool BeAGuid(string guid)
	{
		return Guid.TryParse(guid, out _);
	}
}
