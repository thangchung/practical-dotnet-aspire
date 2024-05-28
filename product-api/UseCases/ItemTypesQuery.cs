using CoffeeShop.Shared.Endpoint;

using ProductApi.Domain;
using ProductApi.Dto;

namespace ProductApi.UseCases;

public class ItemTypesEndpoint : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapGet("item-types",
			async (ISender sender) =>
				await sender.Send(new ItemTypesQuery()));
	}
}

public record ItemTypesQuery : IRequest<IEnumerable<ItemTypeDto>>;

internal class ItemTypesQueryValidator : AbstractValidator<ItemTypesQuery>
{
}

internal class ItemTypesQueryHandler(ILogger<ItemTypesQueryHandler> logger) : IRequestHandler<ItemTypesQuery, IEnumerable<ItemTypeDto>>
{
	public Task<IEnumerable<ItemTypeDto>> Handle(ItemTypesQuery request, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(request);

		var results = new List<ItemTypeDto>
		{
            // beverages
            new() {Name = ItemType.CAPPUCCINO.ToString(), ItemType = ItemType.CAPPUCCINO},
			new() {Name = ItemType.COFFEE_BLACK.ToString(), ItemType = ItemType.COFFEE_BLACK},
			new() {Name = ItemType.COFFEE_WITH_ROOM.ToString(), ItemType = ItemType.COFFEE_WITH_ROOM},
			new() {Name = ItemType.ESPRESSO.ToString(), ItemType = ItemType.ESPRESSO},
			new() {Name = ItemType.ESPRESSO_DOUBLE.ToString(), ItemType = ItemType.ESPRESSO_DOUBLE},
			new() {Name = ItemType.LATTE.ToString(), ItemType = ItemType.LATTE},
            // food
            new() {Name = ItemType.CAKEPOP.ToString(), ItemType = ItemType.CAKEPOP},
			new() {Name = ItemType.CROISSANT.ToString(), ItemType = ItemType.CROISSANT},
			new() {Name = ItemType.MUFFIN.ToString(), ItemType = ItemType.MUFFIN},
			new() {Name = ItemType.CROISSANT_CHOCOLATE.ToString(), ItemType = ItemType.CROISSANT_CHOCOLATE}
		};

		return Task.FromResult(results.Distinct());
	}
}