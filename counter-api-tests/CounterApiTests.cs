using System.Text.Json;

using Asp.Versioning;
using Asp.Versioning.Http;

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
}
