using CoffeeShop.Shared.Endpoint;

using ProductApi.Domain;
using ProductApi.Dto;

namespace ProductApi.UseCases;

public class ItemsByIdsEndpoint : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapGet("items-by-types/{itemTypes}",
			async (ISender sender, string itemTypes) =>
				await sender.Send(new ItemsByIdsQuery(itemTypes)));
	}
}

public record ItemsByIdsQuery(string ItemTypes) : IRequest<IEnumerable<ItemDto>>;

internal class ItemsByIdsQueryValidator : AbstractValidator<ItemsByIdsQuery>
{
	public ItemsByIdsQueryValidator()
	{
		RuleFor(v => v.ItemTypes)
			.NotEmpty().WithMessage("ItemTypes is required.");
	}
}

internal class ItemsByIdsQueryHandler(ILogger<ItemsByIdsQueryHandler> logger) : IRequestHandler<ItemsByIdsQuery, IEnumerable<ItemDto>>
{
	public Task<IEnumerable<ItemDto>> Handle(ItemsByIdsQuery request, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(request);

		var results = new List<ItemDto>();
		var itemTypes = request.ItemTypes.Split(",").Select(id => (ItemType)Convert.ToInt16(id));
		foreach (var itemType in itemTypes)
		{
			var temp = Item.GetItem(itemType);
			results.Add(new ItemDto(temp.Type, temp.Price));
		}

		return Task.FromResult(results.Distinct());
	}
}