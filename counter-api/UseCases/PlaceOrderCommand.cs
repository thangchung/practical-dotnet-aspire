using FluentValidation;
using MediatR;
using CounterApi.Domain.Commands;
using CounterApi.Domain;
using System.Text.Json;
using MassTransit;
using CounterApi.Domain.SharedKernel;
using CoffeeShop.MessageContracts;
using CounterApi.Domain.DomainEvents;
using CounterApi.Domain.Dtos;

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

internal class PlaceOrderHandler(IPublishEndpoint publisher, IItemGateway itemGateway, ILogger<PlaceOrderHandler> logger)
    : IRequestHandler<PlaceOrderCommand, IResult>
{
    public async Task<IResult> Handle(PlaceOrderCommand placeOrderCommand, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(placeOrderCommand);

        var itemTypes = new List<ItemType> { ItemType.ESPRESSO };
        var items = await itemGateway.GetItemsByType(itemTypes.ToArray());
        logger.LogInformation("[ProductAPI] Query: {JsonObject}", JsonSerializer.Serialize(items));

        var orderId = Guid.NewGuid().ToString();

        var order = await Order.From(placeOrderCommand, itemGateway);
        order.Id = new Guid(orderId);

        // map domain object to dto
        var dto = Order.ToDto(order);
        dto.OrderStatus = OrderStatus.IN_PROGRESS;

        logger.LogInformation("Got {count} domain events.", order.DomainEvents.Count);
        var @events = new IDomainEvent[order.DomainEvents.Count];
        order.DomainEvents.CopyTo(@events);
        order.DomainEvents.Clear();

        var baristaEvents = new Dictionary<Guid, BaristaOrderPlaced>();
        var kitchenEvents = new Dictionary<Guid, KitchenOrderPlaced>();
        foreach (var @event in @events)
        {
            switch (@event)
            {
                case BaristaOrderIn baristaOrderInEvent:
                    if (!baristaEvents.TryGetValue(baristaOrderInEvent.OrderId, out _))
                    {
                        baristaEvents.Add(baristaOrderInEvent.OrderId, new BaristaOrderPlaced
                        {
                            OrderId = baristaOrderInEvent.OrderId,
                            ItemLines =
                            [
                                new(baristaOrderInEvent.ItemLineId, baristaOrderInEvent.ItemType, ItemStatus.IN_PROGRESS)
                            ]
                        });
                    }
                    else
                    {
                        baristaEvents[baristaOrderInEvent.OrderId].ItemLines.Add(
                            new OrderItemLineDto(baristaOrderInEvent.ItemLineId, baristaOrderInEvent.ItemType, ItemStatus.IN_PROGRESS));
                    }

                    break;
                case KitchenOrderIn kitchenOrderInEvent:
                    if (!kitchenEvents.TryGetValue(kitchenOrderInEvent.OrderId, out _))
                    {
                        kitchenEvents.Add(kitchenOrderInEvent.OrderId, new KitchenOrderPlaced
                        {
                            OrderId = kitchenOrderInEvent.OrderId,
                            ItemLines =
                            [
                                new(kitchenOrderInEvent.ItemLineId, kitchenOrderInEvent.ItemType, ItemStatus.IN_PROGRESS)
                            ]
                        });
                    }
                    else
                    {
                        kitchenEvents[kitchenOrderInEvent.OrderId].ItemLines.Add(
                            new OrderItemLineDto(kitchenOrderInEvent.ItemLineId, kitchenOrderInEvent.ItemType, ItemStatus.IN_PROGRESS));
                    }

                    break;
            }
        }

        if (baristaEvents.Count > 0)
        {
            logger.LogInformation("Pushlish barista events.");
            foreach(var @event in baristaEvents) {
                await publisher.Publish(@event.Value, cancellationToken);
            }
        }

        if (kitchenEvents.Count > 0)
        {
            logger.LogInformation("Pushlish kitchen events.");
            foreach(var @event in kitchenEvents) {
                await publisher.Publish(@event.Value, cancellationToken);
            }
        }

        return Results.Ok();
    }
}
