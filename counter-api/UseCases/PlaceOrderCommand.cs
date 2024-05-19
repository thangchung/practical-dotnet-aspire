using FluentValidation;
using MediatR;
using CounterApi.Domain.Commands;
using CounterApi.Domain;
using System.Text.Json;
using CoffeeShop.Shared.Domain;

namespace CounterApi.UseCases;

public static class OrderInRouteMapper
{
	public static IEndpointRouteBuilder MapOrderInApiRoutes(this IEndpointRouteBuilder builder)
	{
		builder.MapPost("/v1/api/orders", async (PlaceOrderCommand command, ISender sender) => await sender.Send(command));
		return builder;
	}
}

internal class OrderInValidator : AbstractValidator<PlaceOrderCommand>
{
}

internal class PlaceOrderHandler(IPublisher publisher, IItemGateway itemGateway, ILogger<PlaceOrderHandler> logger)
	: IRequestHandler<PlaceOrderCommand, IResult>
{
	public async Task<IResult> Handle(PlaceOrderCommand placeOrderCommand, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(placeOrderCommand);

		var itemTypes = new List<ItemType> { ItemType.ESPRESSO };
		var items = await itemGateway.GetItemsByType(itemTypes.ToArray());
		logger.LogInformation("[ProductAPI] Query: {JsonObject}", JsonSerializer.Serialize(items));

		var order = await Order.From(placeOrderCommand, itemGateway);
		order.DomainEventAggregation();
		await order.RelayAndPublishEvents(publisher, cancellationToken);

		return Results.Ok();
	}
}
