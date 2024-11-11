namespace CoffeeShop.Shared.Helpers;

[DebuggerStepThrough]
public static class GuidHelper
{
	public static Guid NewGuid()
	{
		return Guid.CreateVersion7();
	}

	public static bool BeAGuid(string guid)
	{
		return Guid.TryParse(guid, out _);
	}
}
