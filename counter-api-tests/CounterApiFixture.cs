using Aspire.Hosting.Testing;

using CounterApi.Domain;
using CounterApi.IntegrationEvents.EventHandlers;

using MassTransit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace CoffeeShop.CounterApi.IntegrationTests;

public sealed class CounterApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
	private readonly IHost _app;

	public IResourceBuilder<PostgresServerResource> Postgres { get; private set; }
	private string _postgresConnectionString;

	public IResourceBuilder<RabbitMQServerResource> RabbitMq { get; private set; }
	private string _rabbitMqConnectionString;

	public IResourceBuilder<WireMockServerResource> ProductApi { get; private set; }

	public CounterApiFixture()
	{
		var options = new DistributedApplicationOptions { AssemblyName = typeof(CounterApiFixture).Assembly.FullName, DisableDashboard = true };
		var appBuilder = DistributedApplication.CreateBuilder(options);

		Postgres = appBuilder.AddPostgres("postgresQL");
		RabbitMq = appBuilder.AddRabbitMQ("rabbitmq");
		
		ProductApi = appBuilder.AddWireMock("product-api", WireMockServerArguments.DefaultPort)
			.WithApiMappingBuilder(ProductApiMock.BuildAsync);

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
				{ "ProductApiUrl", ProductApi.GetEndpoint("http").Url }
			}!);
		})
		.ConfigureWebHost(builder =>
		{
			builder.UseTestServer()
				.ConfigureServices(services =>
				{
					services.RemoveAll<IHostedService>();
				})
				.ConfigureTestServices(services =>
				{
					services.AddMassTransitTestHarness(x =>
					{
						x.AddConsumer<BaristaOrderUpdatedConsumer>();
						x.AddConsumer<KitchenOrderUpdatedConsumer>();
					});
				});
		});

		return base.CreateHost(builder);
	}

    public async Task InitializeAsync()
    {
        await _app.StartAsync();

        _postgresConnectionString = await Postgres.Resource.GetConnectionStringAsync() ?? throw new InvalidOperationException("Postgres connection string is null");
        _rabbitMqConnectionString = await RabbitMq.Resource.ConnectionStringExpression.GetValueAsync(default) ?? throw new InvalidOperationException("RabbitMQ connection string is null");
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
	public static async Task BuildAsync(AdminApiMappingBuilder builder)
	{
		builder.Given(b => b
			.WithRequest(request => request
				.UsingGet()
				.WithPath("/api/v1/item-types")
			)
			.WithResponse(response => response
				.WithHeaders(h => h.Add("Content-Type", "application/json"))
				.WithBodyAsJson(() => new List<ItemTypeDto> {
					new() {
						ItemType = ItemType.CAKEPOP
					},
					new() {
						ItemType = ItemType.CAPPUCCINO
					}
				}.ToArray()
			)
		));

		await builder.BuildAndPostAsync();
	}
}

public class ItemTypeDto
{
	public ItemType ItemType { get; set; }
	public string Name { get; set; } = null!;
}