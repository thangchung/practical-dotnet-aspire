using CoffeeShop.Shared.Domain;
using CoffeeShop.Shared.Endpoint;
using CoffeeShop.Shared.OpenTelemetry;

using CounterApi.Domain;
using CounterApi.Domain.Commands;

namespace CounterApi.UseCases;

public class OrderInEndpoint : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("orders", async (PlaceOrderCommand command, ISender sender) => await sender.Send(command));
	}
}

internal class OrderInValidator : AbstractValidator<PlaceOrderCommand>
{
	public OrderInValidator()
	{
		RuleFor(command => command.OrderId)
			.NotEmpty().WithMessage("The order identifier can't be empty.");
	}
}

// [IgnoreOTelOnHandler]
internal class PlaceOrderHandler(IPublisher publisher, IItemGateway itemGateway, ILogger<PlaceOrderHandler> logger)
	: IRequestHandler<PlaceOrderCommand, IResult>
{
	public async Task<IResult> Handle(PlaceOrderCommand placeOrderCommand, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(placeOrderCommand);

		var itemTypes = new List<ItemType> { ItemType.ESPRESSO }; // todo: remove hard-code
		var items = await itemGateway.GetItemsByType(itemTypes.ToArray());
		logger.LogInformation("[ProductAPI] Query: {JsonObject}", JsonSerializer.Serialize(items));

		var order = await Order.From(placeOrderCommand, itemGateway);
		order.DomainEventAggregation();
		await order.RelayAndPublishEvents(publisher, cancellationToken);

		return Results.Ok();
	}
}
