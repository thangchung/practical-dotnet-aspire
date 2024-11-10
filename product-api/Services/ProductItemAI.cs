
using Microsoft.Extensions.AI;
using System.Diagnostics;

using Pgvector;

using ProductApi.Domain;

namespace ProductApi.Services;

/// <param name="environment">The web host environment.</param>
public class ProductItemAI(
	IWebHostEnvironment environment, 
	ILogger<ProductItemAI> logger, 
	IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator = null) 
	: IProductItemAI
{
	private const int EmbeddingDimensions = 384;

	/// <summary>Logger for use in AI operations.</summary>
	private readonly ILogger _logger = logger;

	/// <inheritdoc/>
	public bool IsEnabled => embeddingGenerator is not null;

	/// <inheritdoc/>
	public ValueTask<Vector> GetEmbeddingAsync(ItemV2 item) =>
		IsEnabled ?
			GetEmbeddingAsync(CatalogItemToString(item)) :
			ValueTask.FromResult<Vector>(null);

	/// <inheritdoc/>
	public async ValueTask<IReadOnlyList<Vector>> GetEmbeddingsAsync(IEnumerable<ItemV2> items)
	{
		if (IsEnabled)
		{
			long timestamp = Stopwatch.GetTimestamp();

			GeneratedEmbeddings<Embedding<float>> embeddings = await embeddingGenerator.GenerateAsync(items.Select(CatalogItemToString));
			var results = embeddings.Select(m => new Vector(m.Vector[0..EmbeddingDimensions])).ToList();

			if (_logger.IsEnabled(LogLevel.Trace))
			{
				_logger.LogTrace("Generated {EmbeddingsCount} embeddings in {ElapsedMilliseconds}s", results.Count, Stopwatch.GetElapsedTime(timestamp).TotalSeconds);
			}

			return results;
		}

		return null;
	}

	/// <inheritdoc/>
	public async ValueTask<Vector> GetEmbeddingAsync(string text)
	{
		if (IsEnabled)
		{
			long timestamp = Stopwatch.GetTimestamp();

			var embedding = (await embeddingGenerator.GenerateAsync(text))[0].Vector;
			embedding = embedding[0..EmbeddingDimensions];

			if (_logger.IsEnabled(LogLevel.Trace))
			{
				_logger.LogTrace("Generated embedding in {ElapsedMilliseconds}s: '{Text}'", Stopwatch.GetElapsedTime(timestamp).TotalSeconds, text);
			}

			return new Vector(embedding);
		}

		return null;
	}

	//private static string CatalogItemToString(ItemV2 item) => $"{item.Type} {item.Price}";
	private static string CatalogItemToString(ItemV2 item) => $"{item.Type}";
}
