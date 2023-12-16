using CounterApi.Domain.Dtos;

using FluentValidation;
using MediatR;

namespace CounterApi.UseCases;

internal static class OrderFulfillmentRouteMapper
{
    public static IEndpointRouteBuilder MapOrderFulfillmentApiRoutes(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/v1/api/fulfillment-orders", async (ISender sender) => await sender.Send(new OrderFulfillmentQuery()));
        return builder;
    }
}

public record OrderFulfillmentQuery : IRequest<IResult>
{
}

internal class OrderFulfillmentValidator : AbstractValidator<OrderFulfillmentQuery>
{
    public OrderFulfillmentValidator()
    {
    }
}

internal class OrderFulfillmentQueryHandler : IRequestHandler<OrderFulfillmentQuery, IResult>
{
    public async Task<IResult> Handle(OrderFulfillmentQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var orderGuidProcceed = new List<string>();

        // todo
        // var orderGuidList = await daprClient.GetStateAsync<List<Guid>>("statestore", "order-list", cancellationToken: cancellationToken);
        // if (orderGuidList != null && orderGuidList?.Count > 0)
        // {
        //     foreach (var orderGuid in orderGuidList)
        //     {
        //         orderGuidProcceed.Add($"order-{orderGuid}");
        //     }

        //     var mulitpleStateResult = await daprClient.GetBulkStateAsync("statestore", orderGuidProcceed, parallelism: 1, cancellationToken: cancellationToken);

        //     return Results.Ok(mulitpleStateResult.Select(x => JsonSerializer.Serialize<OrderDto>(x.Value)).ToList());
        // }

        return Results.Ok(orderGuidProcceed);
    }
}