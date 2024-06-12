using System.Net.Http.Json;
using System.Text.Json;

using Asp.Versioning;
using Asp.Versioning.Http;

using CoffeeShop.Shared.Helpers;

using CounterApi.Domain;
using CounterApi.Domain.Commands;
using CounterApi.Domain.Dtos;

using Microsoft.AspNetCore.Mvc.Testing;

using Xunit;

namespace CoffeeShop.CounterApi.Tests;

public sealed class CounterApiTests : IClassFixture<CounterApiFixture>
{
	private readonly WebApplicationFactory<Program> _webApplicationFactory;
	private readonly HttpClient _httpClient;
	private readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);

	public CounterApiTests(CounterApiFixture fixture)
	{
		//var handler = new ApiVersionHandler(new QueryStringApiVersionWriter(), new ApiVersion(1.0));

		_webApplicationFactory = fixture;
		_httpClient = _webApplicationFactory.CreateDefaultClient();
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
			BaristaItems = [new CommandItem { ItemType = ItemType.CAPPUCCINO}],
			KitchenItems = [new CommandItem { ItemType = ItemType.CAKEPOP }],
		};
		var response = await _httpClient.PostAsJsonAsync("/api/v1/orders", json);
		response.EnsureSuccessStatusCode();
		var body = await response.Content.ReadAsStringAsync();

		Assert.NotNull(body);
	}
}
