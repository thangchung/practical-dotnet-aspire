using Microsoft.AspNetCore.Routing;

namespace CoffeeShop.Shared.Endpoint;

public interface IEndpoint
{
	void MapEndpoint(IEndpointRouteBuilder app);
}
