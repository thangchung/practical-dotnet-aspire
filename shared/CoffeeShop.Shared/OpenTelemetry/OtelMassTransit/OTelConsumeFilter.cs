using Microsoft.AspNetCore.Http;

namespace CoffeeShop.Shared.OpenTelemetry.OtelMassTransit;

public class OTelConsumeFilter<T>(IActivityScope activityScope, IHttpContextAccessor httpContextAccessor) : IFilter<ConsumeContext<T>> where T : class
{
	public void Probe(ProbeContext context)
	{
	}

	public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
	{
		context.TryGetHeader("USER-CONTEXT", out string userContext);

		var temp = httpContextAccessor;
		var activityName = $"{context.Message.GetType().FullName}-enrich";

		await activityScope.Run(
			activityName,
				async (_, token) => await next.Send(context),
				new StartActivityOptions { Tags = { { "USER-CONTEXT", userContext } } },
				default
			);
	}
}
