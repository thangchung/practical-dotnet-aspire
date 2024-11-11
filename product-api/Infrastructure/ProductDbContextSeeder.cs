
using CoffeeShop.Shared.EF;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Pgvector;

using ProductApi.Domain;
using ProductApi.Services;

namespace ProductApi.Infrastructure;

public class ProductDbContextSeeder(
	IWebHostEnvironment env,
	IProductItemAI catalogAI,
	ILogger<ProductDbContextSeeder> logger
	) : IDbSeeder<ProductDbContext>
{
	public async Task SeedAsync(ProductDbContext context)
	{
		// Workaround from https://github.com/npgsql/efcore.pg/issues/292#issuecomment-388608426
		context.Database.OpenConnection();
		((NpgsqlConnection)context.Database.GetDbConnection()).ReloadTypes();

		if (!context.Items.Any())
		{
			await context.Items.ExecuteDeleteAsync();

			var catalogItems = new ItemV2Data();

			if (catalogAI.IsEnabled)
			{
				logger.LogInformation("Generating {NumItems} embeddings", catalogItems.Count);
				IReadOnlyList<Vector> embeddings = await catalogAI.GetEmbeddingsAsync(catalogItems);
				for (int i = 0; i < catalogItems.Count; i++)
				{
					catalogItems[i].Embedding = embeddings[i];
				}
			}

			await context.Items.AddRangeAsync(catalogItems);
			logger.LogInformation("Seeded catalog with {NumItems} items", context.Items.Count());
			await context.SaveChangesAsync();
		}

		await context.SaveChangesAsync();
	}
}
