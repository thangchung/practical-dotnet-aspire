using Pgvector;

using ProductApi.Domain;

namespace ProductApi.Services;

public interface IProductItemAI
{
	bool IsEnabled { get; }

	ValueTask<Vector> GetEmbeddingAsync(string text);

	ValueTask<Vector> GetEmbeddingAsync(ItemV2 item);

	ValueTask<IReadOnlyList<Vector>> GetEmbeddingsAsync(IEnumerable<ItemV2> item);
}
