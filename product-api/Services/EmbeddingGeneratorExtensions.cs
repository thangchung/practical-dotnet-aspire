using System.Data.Common;

namespace ProductApi.Services;

public static class EmbeddingGeneratorExtensions
{
	public static void AddEmbeddingGenerator(this IHostApplicationBuilder builder)
	{
		if (builder.Configuration.GetValue<string>("AI:Type") is string type && type is "ollama")
		{
			var connectionString = builder.Configuration.GetConnectionString(type);
			if (string.IsNullOrWhiteSpace(connectionString))
			{
				throw new InvalidOperationException($"No connection string named '{type}' was found. Ensure a corresponding Aspire service was registered.");
			}
			var connectionStringBuilder = new DbConnectionStringBuilder
			{
				ConnectionString = connectionString
			};
			var endpoint = (string?)connectionStringBuilder["endpoint"];

			// builder.AddOllamaSharpEmbeddingGenerator("embedding");
			builder.Services.AddEmbeddingGenerator(b => b.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>())
				.UseOpenTelemetry()
				.UseLogging()
				.Use((f) => new OllamaEmbeddingGenerator(
					new Uri(endpoint!),
					builder.Configuration.GetValue<string>("AI:EMBEDDINGMODEL")));
		}
		else
		{
			builder.AddAzureOpenAIClient("openai");
			builder.Services.AddEmbeddingGenerator(sp => sp.GetRequiredService<OpenAIClient>().AsEmbeddingGenerator(builder.Configuration["AI:EMBEDDINGMODEL"]!))
				.UseOpenTelemetry()
				.UseLogging()
				.Build();
		}
	}
}
