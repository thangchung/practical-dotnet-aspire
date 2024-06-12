using CounterApi.Domain;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using WireMock.Client.Builders;
using Xunit;

namespace CoffeeShop.CounterApi.Tests;

public sealed class CounterApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
	private readonly IHost _app;

	public IResourceBuilder<PostgresServerResource> Postgres { get; private set; }
	private string _postgresConnectionString;

	public IResourceBuilder<RabbitMQServerResource> RabbitMq { get; private set; }
	private string _rabbitMqConnectionString;

	public IResourceBuilder<WireMockNetResource> ProductApi { get; private set; }
	private string _productApiEndpoint;

	public CounterApiFixture()
	{
		var options = new DistributedApplicationOptions { AssemblyName = typeof(CounterApiFixture).Assembly.FullName, DisableDashboard = true };
		var appBuilder = DistributedApplication.CreateBuilder(options);
		Postgres = appBuilder.AddPostgres("postgresQL");
		RabbitMq = appBuilder.AddRabbitMQ("rabbitmq").WithHealthCheck();
		ProductApi = appBuilder.AddWireMockNet("product-api")
			.WithApiMappingBuilder(ProductApiMock.Build).WithHealthCheck();
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
				{ "ProductApiUrl", _productApiEndpoint }
			}!);
		});
		return base.CreateHost(builder);
	}

	public async Task InitializeAsync()
	{
		await _app.StartAsync();
		_postgresConnectionString = await Postgres.Resource.GetConnectionStringAsync();
		_rabbitMqConnectionString = await RabbitMq.Resource.ConnectionStringExpression.GetValueAsync(default);
		_productApiEndpoint = ProductApi.Resource.PrimaryEndpoint.Url;

		// if don't waiting then WireMock will be failed
		await Task.Delay(TimeSpan.FromSeconds(2));
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

internal class ProductApiMock
{
	public static async Task Build(AdminApiMappingBuilder builder)
	{
		var itemTypes = new List<ItemTypeDto> {
					new() {
						ItemType = ItemType.CAKEPOP
					},
					new() {
						ItemType = ItemType.CAPPUCCINO
					}};

		builder.Given(builder => builder
			.WithRequest(request => request
				.UsingGet()
				.WithPath("/api/v1/item-types")
			)
			.WithResponse(response => response
				.WithBodyAsJson(itemTypes)
			)
		);

		await builder.BuildAndPostAsync();
	}
}

public class ItemTypeDto
{
	public ItemType ItemType { get; set; }
	public string Name { get; set; } = null!;
}