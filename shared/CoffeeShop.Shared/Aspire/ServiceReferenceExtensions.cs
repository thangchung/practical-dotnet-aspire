using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace System.Net.Http;

public static class ServiceReferenceExtensions
{
	public static IHttpClientBuilder AddHttpServiceReference<TClient>(this IServiceCollection services, string baseAddress)
		where TClient : class
	{
		ArgumentNullException.ThrowIfNull(services);

		if (!Uri.IsWellFormedUriString(baseAddress, UriKind.Absolute))
		{
			throw new ArgumentException("Base address must be a valid absolute URI.", nameof(baseAddress));
		}

		return services.AddHttpClient<TClient>(c => c.BaseAddress = new(baseAddress));
	}


	public static IHttpClientBuilder AddHttpServiceReference<TClient>(this IServiceCollection services, string baseAddress, string healthRelativePath, string? healthCheckName = default, HealthStatus failureStatus = default)
		where TClient : class
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentException.ThrowIfNullOrEmpty(healthRelativePath);

		if (!Uri.IsWellFormedUriString(baseAddress, UriKind.Absolute))
		{
			throw new ArgumentException("Base address must be a valid absolute URI.", nameof(baseAddress));
		}

		if (!Uri.IsWellFormedUriString(healthRelativePath, UriKind.Relative))
		{
			throw new ArgumentException("Health check path must be a valid relative URI.", nameof(healthRelativePath));
		}

		var uri = new Uri(baseAddress);
		var builder = services.AddHttpClient<TClient>(c => c.BaseAddress = uri);

		services.AddHealthChecks()
			.AddUrlGroup(
				new Uri(uri, healthRelativePath),
				healthCheckName ?? $"{typeof(TClient).Name}-health",
				failureStatus,
				//configureClient: (s, c) => s.GetRequiredService<IHttpClientFactory>().CreateClient
				configurePrimaryHttpMessageHandler: s => s.GetRequiredService<IHttpMessageHandlerFactory>().CreateHandler()
				);

		return builder;
	}
}