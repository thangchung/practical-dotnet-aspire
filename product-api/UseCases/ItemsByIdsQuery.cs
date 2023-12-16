using FluentValidation;
using MediatR;

using ProductApi.Domain;
using ProductApi.Dtos;

namespace ProductApi.UseCases;

internal static class ItemsByIdsQueryRouter
{
    public static IEndpointRouteBuilder MapItemTypesQueryApiRoutes(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/v1/api/items-by-types/{itemTypes}",
            async (ISender sender, string itemTypes) =>
                await sender.Send(new ItemsByIdsQuery(itemTypes)));
        builder.MapGet("/v1-items-by-types/{itemTypes}",
            async (ISender sender, string itemTypes) =>
                await sender.Send(new ItemsByIdsQuery(itemTypes)));
        return builder;
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