using System.Reflection;

using Microsoft.Extensions.Logging;

namespace CoffeeShop.Shared.OpenTelemetry;

public class HandlerBehavior<TRequest, TResponse>(
	IRequestHandler<TRequest, TResponse> outerHandler,
	IActivityScope activityScope,
	CommandHandlerMetrics metrics,
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

		var tagName = queryName.ToLowerInvariant().EndsWith("command") ? TelemetryTags.Commands.Command : TelemetryTags.QueryHandling.Query;
		var startingTimestamp = metrics.CommandHandlingStart(handlerName);

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
			metrics.CommandHandlingEnd(handlerName, startingTimestamp);
		}
	}
}
