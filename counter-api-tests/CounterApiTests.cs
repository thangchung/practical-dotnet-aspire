using System.Net.Http.Json;
using System.Text.Json;

using Asp.Versioning;
using Asp.Versioning.Http;

using CoffeeShop.MessageContracts;
using CoffeeShop.Shared.Helpers;

using CounterApi.Domain;
using CounterApi.Domain.Commands;
using CounterApi.IntegrationEvents.EventHandlers;

using MassTransit.Testing;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace CoffeeShop.CounterApi.IntegrationTests;

internal class RetryHandler : DelegatingHandler
{
	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		return base.SendAsync(request, cancellationToken);
	}
}

public sealed class CounterApiTests : IClassFixture<CounterApiFixture>
{
	private readonly WebApplicationFactory<Program> _webApplicationFactory;
	private readonly TestServer _host;
	private readonly HttpClient _httpClient;
	private readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);

	public CounterApiTests(CounterApiFixture fixture)
	{
		var handler = new ApiVersionHandler(new QueryStringApiVersionWriter(), new ApiVersion(1.0));
		_webApplicationFactory = fixture;
		_host = fixture.Server;
		_httpClient = _webApplicationFactory.CreateDefaultClient(handler);
	}

	[Fact]
	public async Task GetOrder()
	{
		// Act
		var response = await _httpClient.GetAsync("/api/v1/fulfillment-orders");

		// Assert
		response.EnsureSuccessStatusCode();
		var body = await response.Content.ReadAsStringAsync();
		var result = JsonSerializer.Deserialize<List<string>>(body, _jsonSerializerOptions);

		Assert.NotNull(result);
	}

	[Fact]
	public async Task SubmitOrder()
	{
		var json = new PlaceOrderCommand
		{
			OrderId = GuidHelper.NewGuid(),
			CommandType = CommandType.PLACE_ORDER,
			OrderSource = OrderSource.WEB,
			Location = Location.ATLANTA,
			LoyaltyMemberId = GuidHelper.NewGuid(),
			Timestamp = DateTime.UtcNow,
			BaristaItems = [new CommandItem { ItemType = ItemType.CAPPUCCINO }],
			KitchenItems = [new CommandItem { ItemType = ItemType.CAKEPOP }],
		};

		var response = await _httpClient.PostAsJsonAsync("/api/v1/orders", json);

		response.EnsureSuccessStatusCode();
		var body = await response.Content.ReadAsStringAsync();

		Assert.NotNull(body);

		var testHarness = _host.Services?.GetService<ITestHarness>();
		Assert.NotNull(testHarness);

		await testHarness.Start();

		var orderId = GuidHelper.NewGuid();

		await testHarness.Bus.Publish(new BaristaOrderUpdated { OrderId = orderId });
		await testHarness.Bus.Publish(new KitchenOrderUpdated { OrderId = orderId });

		var consumer1 = testHarness.GetConsumerHarness<BaristaOrderUpdatedConsumer>();
		var consumer2 = testHarness.GetConsumerHarness<KitchenOrderUpdatedConsumer>();

		Assert.True(await consumer1.Consumed.Any<BaristaOrderUpdated>(f => f.Context.Message.OrderId == orderId));
		Assert.True(await consumer2.Consumed.Any<KitchenOrderUpdated>(f => f.Context.Message.OrderId == orderId));
	}
}
