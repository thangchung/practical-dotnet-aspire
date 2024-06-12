using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Xunit;

namespace CoffeeShop.CounterApi.Tests;

public sealed class CounterApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
	private readonly IHost _app;

	public IResourceBuilder<PostgresServerResource> Postgres { get; private set; }
	private string _postgresConnectionString;

	public IResourceBuilder<RabbitMQServerResource> RabbitMq { get; private set; }
	private string _rabbitMqConnectionString;

	public CounterApiFixture()
	{
		var options = new DistributedApplicationOptions { AssemblyName = typeof(CounterApiFixture).Assembly.FullName, DisableDashboard = true };
		var appBuilder = DistributedApplication.CreateBuilder(options);
		Postgres = appBuilder.AddPostgres("postgresQL");
		RabbitMq = appBuilder.AddRabbitMQ("rabbitmq");
		_app = appBuilder.Build();
	}

	protected override IHost CreateHost(IHostBuilder builder)
	{
		builder.ConfigureHostConfiguration(config =>
		{
			config.AddInMemoryCollection(new Dictionary<string, string>
			{
				{ $"ConnectionStrings:{Postgres.Resource.Name}", _postgresConnectionString },
				{ $"ConnectionStrings:{RabbitMq.Resource.Name}", _rabbitMqConnectionString },
			}!);
		});
		return base.CreateHost(builder);
	}

	public async Task InitializeAsync()
	{
		await _app.StartAsync();
		_postgresConnectionString = await Postgres.Resource.GetConnectionStringAsync();
		_rabbitMqConnectionString = await RabbitMq.Resource.ConnectionStringExpression.GetValueAsync(default);
	}

	public new async Task DisposeAsync()
	{
		await base.DisposeAsync();
		await _app.StopAsync();
		if (_app is IAsyncDisposable asyncDisposable)
		{
			await asyncDisposable.DisposeAsync().ConfigureAwait(false);
		}
		else
		{
			_app.Dispose();
		}
	}
}