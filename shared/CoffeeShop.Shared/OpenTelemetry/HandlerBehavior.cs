using System.Reflection;

using Microsoft.Extensions.Logging;

namespace CoffeeShop.Shared.OpenTelemetry;

public class HandlerBehavior<TRequest, TResponse>(
	IRequestHandler<TRequest, TResponse> outerHandler,
	IActivityScope activityScope,
	CommandHandlerMetrics commandMetrics,
	QueryHandlerMetrics queryMetrics,
	ILogger<HandlerBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
	where TRequest : notnull, IRequest<TResponse>
	where TResponse : notnull
{
	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		logger.LogInformation("Handled {RequestName}", typeof(TRequest).FullName);

		var attr = outerHandler
			.GetType().GetCustomAttribute<IgnoreOTelOnHandlerAttribute>();

		if (attr is not null)
		{
			return await next();
		}

		var handlerName = outerHandler.GetType().Name;
		var queryName = typeof(TRequest).Name;
		var activityName = $"{queryName}-{handlerName}";

		var isCommand = queryName.ToLowerInvariant().EndsWith("command");

		var tagName = isCommand ? TelemetryTags.Commands.Command : TelemetryTags.Queries.Query;

		var startingTimestamp = isCommand ? commandMetrics.CommandHandlingStart(handlerName) : queryMetrics.QueryHandlingStart(handlerName);

		try
		{
			return await activityScope.Run(
			activityName,
				async (_, token) => await next(),
				new StartActivityOptions { Tags = { { tagName, queryName } } },
				cancellationToken
			);
		}
		finally
		{
			if (isCommand) 
			{
				commandMetrics.CommandHandlingEnd(handlerName, startingTimestamp);
			}
			else {
				queryMetrics.QueryHandlingEnd(handlerName, startingTimestamp);
			}
		}
	}
}
