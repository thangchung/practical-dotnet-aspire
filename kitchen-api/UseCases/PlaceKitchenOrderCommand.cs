// using KitchenApi.Domain;
// using KitchenApi.Domain.Dtos;
// using KitchenApi.Domain.SharedKernel;
// using KitchenApi.Domain.DomainEvents;

// using FluentValidation;
// using MediatR;
// using System.Text.Json;

// namespace KitchenApi.UseCases;

// public static class OrderOrderedRouteMapper
// {
//     public static IEndpointRouteBuilder MapOrderUpApiRoutes(this IEndpointRouteBuilder builder)
//     {
//         // todo

//         return builder;
//     }
// }

// public record PlaceKitchenOrderCommand(Guid OrderId, List<OrderItemDto> ItemLines) : IRequest<IResult>;

// internal class PlaceKitchenOrderCommandValidator : AbstractValidator<PlaceKitchenOrderCommand>
// {
// }

// public class PlaceKitchenOrderCommandHandler(ILogger<PlaceKitchenOrderCommandHandler> logger) : IRequestHandler<PlaceKitchenOrderCommand, IResult>
// {
//     private readonly ILogger<PlaceKitchenOrderCommandHandler> _logger = logger;

//     public async Task<IResult> Handle(PlaceKitchenOrderCommand request, CancellationToken cancellationToken)
//     {
//         ArgumentNullException.ThrowIfNull(request);

//         _logger.LogInformation("Order info: {OrderInfo}", JsonSerializer.Serialize(request));

//         var message = new KitchenOrderUpdated
//         {
//             OrderId = request.OrderId
//         };

//         foreach (var itemLine in request.ItemLines)
//         {
//             // var kitchenOrder = KitchenOrder.From(request.OrderId, itemLine.ItemType, DateTime.UtcNow);

//             await Task.Delay(CalculateDelay(itemLine.ItemType), cancellationToken);

//             // var kitchenItemState = kitchenOrder.SetTimeUp(itemLine.ItemLineId, DateTime.UtcNow);

//             // todo
//             // await daprClient.SaveStateAsync("statestore", $"order-{request.OrderId}", request, cancellationToken: cancellationToken);

//             message.ItemLines.Add(new OrderItemDto(itemLine.ItemLineId, itemLine.ItemType));

//             // if (kitchenItemState.DomainEvents is not null)
//             // {
//             //     var @events = new IDomainEvent[kitchenItemState.DomainEvents.Count];
//             //     kitchenItemState.DomainEvents.CopyTo(@events);
//             //     kitchenItemState.DomainEvents.Clear();

//             //     var message = new KitchenOrderUpdated
//             //     {
//             //         OrderId = request.OrderId
//             //     };
//             //     foreach (var @event in @events)
//             //     {
//             //         if (@event is KitchenOrderUp kitchenOrderUp)
//             //         {
//             //             message.ItemLines.Add(new OrderItemDto(kitchenOrderUp.ItemLineId, kitchenOrderUp.ItemType));
//             //         }
//             //     }

//             //     await daprClient.PublishEventAsync(
//             //                 "kitchenpubsub",
//             //                 "kitchenorderupdated",
//             //                 message,
//             //                 cancellationToken);
//             // }
//         }

//         // todo
//         // await daprClient.PublishEventAsync(
//         //                     "kitchenpubsub",
//         //                     "kitchenorderupdated",
//         //                     message,
//         //                     cancellationToken);

//         return Results.Ok();
//     }

//     private static TimeSpan CalculateDelay(ItemType itemType)
//     {
//         return itemType switch
//         {
//             ItemType.CROISSANT => TimeSpan.FromSeconds(7),
//             ItemType.CROISSANT_CHOCOLATE => TimeSpan.FromSeconds(7),
//             ItemType.CAKEPOP => TimeSpan.FromSeconds(5),
//             ItemType.MUFFIN => TimeSpan.FromSeconds(7),
//             _ => TimeSpan.FromSeconds(3)
//         };
//     }
// }