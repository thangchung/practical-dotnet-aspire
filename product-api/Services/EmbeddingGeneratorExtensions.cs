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

			builder.Services.AddEmbeddingGenerator<string, Embedding<float>>(b => b
				.UseOpenTelemetry()
				.UseLogging()
				.Use(new OllamaEmbeddingGenerator(
					new Uri(endpoint!),
					builder.Configuration.GetValue<string>("AI:EMBEDDINGMODEL"))));
		}
		else
		{
			builder.AddAzureOpenAIClient("openai");
			builder.Services.AddEmbeddingGenerator<string, Embedding<float>>(b => b
				.UseOpenTelemetry()
				.UseLogging()
				.Use(b.Services.GetRequiredService<OpenAIClient>().AsEmbeddingGenerator(builder.Configuration.GetValue<string>("AI:EMBEDDINGMODEL")!)));
		}
	}
}
