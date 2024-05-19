namespace CoffeeShop.Shared.Helpers;

public static class GuidHelper
{
	[DebuggerStepThrough]
	public static Guid NewGuid()
	{
		return NewId.NextGuid();
	}

	public static bool BeAGuid(string guid)
	{
		return Guid.TryParse(guid, out _);
	}
}
