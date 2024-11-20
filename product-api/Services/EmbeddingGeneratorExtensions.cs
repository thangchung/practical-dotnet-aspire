namespace ProductApi.Services;

public static class EmbeddingGeneratorExtensions
{
	public static void AddEmbeddingGenerator(this IHostApplicationBuilder builder)
	{
		if (builder.Configuration.GetValue<string>("AI:Type") is string type && type is "ollama")
		{
			builder.Services.AddEmbeddingGenerator<string, Embedding<float>>(b => b
				.UseOpenTelemetry()
				.UseLogging()
				.Use(new OllamaEmbeddingGenerator(
					new Uri(builder.Configuration["AI:OLLAMA:Endpoint"]!),
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
