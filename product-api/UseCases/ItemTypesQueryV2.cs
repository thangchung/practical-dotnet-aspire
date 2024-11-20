using CoffeeShop.Shared.Endpoint;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Pgvector.EntityFrameworkCore;

using ProductApi.Dto;
using ProductApi.Infrastructure;
using ProductApi.Services;

namespace ProductApi.UseCases;

public class ItemTypesQueryV2
{
	public class ItemTypesEndpoint : IEndpoint
	{
		public void MapEndpoint(IEndpointRouteBuilder app)
		{
			app.MapGet("item-types",
				async (ISender sender, HttpContext httpContext) =>
					await sender.Send(new ItemTypesQuery { Text = httpContext.Request.Query["q"] })).HasApiVersion(2);
		}
	}

	public record ItemTypesQuery : IRequest<IEnumerable<ItemTypeDto>>
	{
		[FromQuery(Name = "q")]
		public string? Text { get; set; }
	}

	internal class ItemTypesQueryValidator : AbstractValidator<ItemTypesQuery>
	{
	}

	internal class ItemTypesQueryHandler(ProductDbContext dbContext, IProductItemAI productItemAI, ILogger<ItemTypesQueryHandler> logger) : IRequestHandler<ItemTypesQuery, IEnumerable<ItemTypeDto>>
	{
		public async Task<IEnumerable<ItemTypeDto>> Handle(ItemTypesQuery request, CancellationToken cancellationToken)
		{
			ArgumentNullException.ThrowIfNull(request);

			request.Text = request.Text ?? "coffee";

			var vector = await productItemAI.GetEmbeddingAsync(request.Text);

			var itemsWithDistance = await dbContext.Items
				.Select(c => new { Item = c, Distance = c.Embedding.CosineDistance(vector) })
				.OrderBy(c => c.Distance)
				.ToListAsync();

			logger.LogDebug("Results from {text}: {results}", request.Text, string.Join(", ", itemsWithDistance.Select(i => $"{i.Item.Type} => {i.Distance}")));

			return await Task.FromResult(itemsWithDistance.Select(x => new ItemTypeDto
			{
				Name = x.Item.Type.ToString(),
				ItemType = x.Item.Type
			}).Distinct());
		}
	}
}