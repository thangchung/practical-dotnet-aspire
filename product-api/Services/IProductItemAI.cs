using Pgvector;

using ProductApi.Domain;

namespace ProductApi.Services;

public interface IProductItemAI
{
	/// <summary>Gets whether the AI system is enabled.</summary>
	bool IsEnabled { get; }

	/// <summary>Gets an embedding vector for the specified text.</summary>
	ValueTask<Vector> GetEmbeddingAsync(string text);

	/// <summary>Gets an embedding vector for the specified catalog item.</summary>
	ValueTask<Vector> GetEmbeddingAsync(ItemV2 item);

	/// <summary>Gets embedding vectors for the specified catalog items.</summary>
	ValueTask<IReadOnlyList<Vector>> GetEmbeddingsAsync(IEnumerable<ItemV2> item);
}
