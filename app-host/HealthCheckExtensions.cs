using Aspirant.Hosting;

using HealthChecks.NpgSql;
using HealthChecks.RabbitMQ;
using HealthChecks.Redis;
using HealthChecks.Uris;

namespace CoffeeShop.AppHost;

/// <summary>
/// Ref: https://github.com/davidfowl/WaitForDependenciesAspire/tree/main/WaitForDependencies.Aspire.Hosting
/// </summary>
public static class Extensions
{
	public static IResourceBuilder<RabbitMQServerResource> WithHealthCheck(this IResourceBuilder<RabbitMQServerResource> builder)
	{
		return builder.WithAnnotation(HealthCheckAnnotation.Create(cs => new RabbitMQHealthCheck(new RabbitMQHealthCheckOptions { ConnectionUri = new(cs) })));
	}

	public static IResourceBuilder<RedisResource> WithHealthCheck(this IResourceBuilder<RedisResource> builder)
	{
		return builder.WithAnnotation(HealthCheckAnnotation.Create(cs => new RedisHealthCheck(cs)));
	}

	public static IResourceBuilder<PostgresServerResource> WithHealthCheck(this IResourceBuilder<PostgresServerResource> builder)
	{
		return builder.WithAnnotation(HealthCheckAnnotation.Create(cs => new NpgSqlHealthCheck(new NpgSqlHealthCheckOptions(cs))));
	}

	public static IResourceBuilder<T> WithHealthCheck<T>(
		this IResourceBuilder<T> builder,
		string? endpointName = null,
		string path = "health",
		Action<UriHealthCheckOptions>? configure = null)
		where T : IResourceWithEndpoints
	{
		return builder.WithAnnotation(new HealthCheckAnnotation(async (resource, ct) =>
		{
			if (resource is not IResourceWithEndpoints resourceWithEndpoints)
			{
				return null;
			}

			var endpoint = endpointName is null
			 ? resourceWithEndpoints.GetEndpoints().FirstOrDefault(e => e.Scheme is "http" or "https")
			 : resourceWithEndpoints.GetEndpoint(endpointName);

			var url = endpoint?.Url;

			if (url is null)
			{
				return null;
			}

			var options = new UriHealthCheckOptions();

			options.AddUri(new(new(url), path));

			configure?.Invoke(options);

			var client = new HttpClient();
			return new UriHealthCheck(options, () => client);
		}));
	}
}