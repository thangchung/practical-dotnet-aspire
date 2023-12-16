using FluentValidation;
using MediatR;

using CounterApi.Domain.Dtos;
using CounterApi.Domain.Messages;
using System.Text.Json;

namespace CounterApi.UseCases;

public static class OrderUpRouteMapper
{
    public static IEndpointRouteBuilder MapOrderUpApiRoutes(this IEndpointRouteBuilder builder)
    {
        // todo

        return builder;
    }
}

public record OrderUpdatedCommand(Guid OrderId, List<OrderItemLineDto> ItemLines, bool IsBarista = true) : IRequest<IResult>;

internal class OrderUpdatedCommandValidator : AbstractValidator<OrderUpdatedCommand>
{
}

internal class OrderUpdatedCommandHandler(ILogger<OrderUpdatedCommandHandler> logger) : IRequestHandler<OrderUpdatedCommand, IResult>
{
    public async Task<IResult> Handle(OrderUpdatedCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("OrderUpdatedCommand received: {OrderUpdatedCommand}",
            JsonSerializer.Serialize(request));

        // todo
        // if (request.IsBarista)
        // {
        //     await daprClient.RaiseWorkflowEventAsync(
        //         request.OrderId.ToString(),
        //         "dapr",
        //         "BaristaOrderUpdated",
        //         request,
        //         cancellationToken
        //     );
        // }
        // else
        // {
        //     await daprClient.RaiseWorkflowEventAsync(
        //         request.OrderId.ToString(),
        //         "dapr",
        //         "KitchenOrderUpdated",
        //         request,
        //         cancellationToken
        //     );
        // }

        return Results.Ok();
    }
}