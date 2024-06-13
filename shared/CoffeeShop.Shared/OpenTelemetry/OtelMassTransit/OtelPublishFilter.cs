using Microsoft.AspNetCore.Http;

namespace CoffeeShop.Shared.OpenTelemetry.OtelMassTransit;

public class OtelPublishFilter<T>(IActivityScope activityScope, IHttpContextAccessor httpContextAccessor) : IFilter<PublishContext<T>> where T : class
{
	public void Probe(ProbeContext context)
	{
	}

	public async Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
	{
		context.Headers.Set("USER-CONTEXT", "some context", overwrite: true);

		var temp = httpContextAccessor;
		var activityName = $"{context.Message.GetType().FullName}-enrich";

		await activityScope.Run(
			activityName,
				async (_, token) => await next.Send(context),
				new StartActivityOptions { Tags = { { "USER-CONTEXT", "some context" } } },
				default
			);
	}
}
