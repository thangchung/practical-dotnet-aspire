using System.Text.Json;

using BaristaApi.Domain;
using BaristaApi.Domain.DomainEvents;
using BaristaApi.Domain.Dtos;
using BaristaApi.Domain.Messages;
using BaristaApi.Domain.SharedKernel;

using FluentValidation;
using MediatR;

namespace BaristaApi.UseCases;

public static class OrderOrderedRouteMapper
{
    public static IEndpointRouteBuilder MapOrderUpApiRoutes(this IEndpointRouteBuilder builder)
    {

        // todo

        return builder;
    }
}

public record PlaceBaristaOrderCommand(Guid OrderId, List<OrderItemDto> ItemLines) : IRequest<IResult>;

internal class PlaceBaristaOrderCommandValidator : AbstractValidator<PlaceBaristaOrderCommand>
{
}

internal class PlaceBaristaOrderCommandHandler(ILogger<PlaceBaristaOrderCommandHandler> logger) : IRequestHandler<PlaceBaristaOrderCommand, IResult>
{
    public async Task<IResult> Handle(PlaceBaristaOrderCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        logger.LogInformation("Order info: {OrderInfo}", JsonSerializer.Serialize(request));

        var message = new BaristaOrderUpdated
        {
            OrderId = request.OrderId
        };

        foreach (var itemLine in request.ItemLines)
        {
            // var baristaItem = BaristaItem.From(itemLine.ItemType, itemLine.ItemType.ToString(), DateTime.UtcNow);

            await Task.Delay(CalculateDelay(itemLine.ItemType), cancellationToken);

            // var baristaItemState = baristaItem.SetTimeUp(request.OrderId, itemLine.ItemLineId, DateTime.UtcNow);

            // todo
            // await daprClient.SaveStateAsync("statestore", $"order-{request.OrderId}", request, cancellationToken: cancellationToken);

            message.ItemLines.Add(new OrderItemDto(itemLine.ItemLineId, itemLine.ItemType));

            // if (baristaItemState.DomainEvents is not null)
            // {
            //     var @events = new IDomainEvent[baristaItemState.DomainEvents.Count];
            //     baristaItemState.DomainEvents.CopyTo(@events);
            //     baristaItemState.DomainEvents.Clear();

            //     var message = new BaristaOrderUpdated
            //     {
            //         OrderId = request.OrderId
            //     };
            //     foreach (var @event in @events)
            //     {
            //         if (@event is BaristaOrderUp baristaOrderUp)
            //         {
            //             message.ItemLines.Add(new OrderItemDto(baristaOrderUp.ItemLineId, baristaOrderUp.ItemType));
            //         }
            //     }

            //     await daprClient.PublishEventAsync(
            //                 "baristapubsub",
            //                 "baristaorderupdated",
            //                 message,
            //                 cancellationToken);
            // }
        }

        // todo
        // await daprClient.PublishEventAsync(
        //                     "baristapubsub",
        //                     "baristaorderupdated",
        //                     message,
        //                     cancellationToken);

        return Results.Ok();
    }

    private static TimeSpan CalculateDelay(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.COFFEE_BLACK => TimeSpan.FromSeconds(5),
            ItemType.COFFEE_WITH_ROOM => TimeSpan.FromSeconds(5),
            ItemType.ESPRESSO => TimeSpan.FromSeconds(7),
            ItemType.ESPRESSO_DOUBLE => TimeSpan.FromSeconds(7),
            ItemType.CAPPUCCINO => TimeSpan.FromSeconds(10),
            _ => TimeSpan.FromSeconds(3)
        };
    }
}