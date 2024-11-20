using System.Diagnostics;

using Pgvector;

using ProductApi.Domain;

namespace ProductApi.Services;

public class ProductItemAI(
	IWebHostEnvironment environment, 
	ILogger<ProductItemAI> logger, 
	IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator = null) 
	: IProductItemAI
{
	private const int EmbeddingDimensions = 384;

	private readonly ILogger _logger = logger;

	public bool IsEnabled => embeddingGenerator is not null;

	public ValueTask<Vector> GetEmbeddingAsync(ItemV2 item) =>
		IsEnabled ?
			GetEmbeddingAsync(CatalogItemToString(item)) :
			ValueTask.FromResult<Vector>(null);

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

	public async ValueTask<Vector> GetEmbeddingAsync(string text)
	{
		if (IsEnabled)
		{
			long timestamp = Stopwatch.GetTimestamp();

			var embedding = (await embeddingGenerator.GenerateEmbeddingAsync(text)).Vector;
			embedding = embedding[0..EmbeddingDimensions];

			if (_logger.IsEnabled(LogLevel.Trace))
			{
				_logger.LogTrace("Generated embedding in {ElapsedMilliseconds}s: '{Text}'", Stopwatch.GetElapsedTime(timestamp).TotalSeconds, text);
			}

			return new Vector(embedding);
		}

		return null;
	}

	private string CatalogItemToString(ItemV2 item)
	{
		_logger.LogDebug("{item.Type} {item.Description}", item.Type, item.Description);
		return $"{item.Type} {item.Description}";
	}
}
