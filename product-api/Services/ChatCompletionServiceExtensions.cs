using System.Runtime.CompilerServices;

using Microsoft.Extensions.AI;

namespace ProductApi.Services;

/// <summary>
/// Ref: https://github.dev/dotnet/eShopSupport
/// </summary>
public static class ChatCompletionServiceExtensions
{
	public static void AddChatCompletionService(this IHostApplicationBuilder builder, string serviceName)
	{
		var pipeline = (ChatClientBuilder pipeline) => pipeline
			.UseFunctionInvocation()
			.UseOpenTelemetry(configure: c => c.EnableSensitiveData = true);

		builder.AddOllamaChatClient(serviceName, pipeline);

		//if (builder.Configuration[$"{serviceName}:Type"] == "ollama")
		//{
		//	builder.AddOllamaChatClient(serviceName, pipeline);
		//}
		//else
		//{
		//	builder.AddOpenAIChatClient(serviceName, pipeline);
		//}
	}

	public static IServiceCollection AddOllamaChatClient(
		this IHostApplicationBuilder hostBuilder,
		string serviceName,
		Func<ChatClientBuilder, ChatClientBuilder>? builder = null,
		string? modelName = null)
	{
		if (modelName is null)
		{
			// var configKey = $"{serviceName}:LlmModelName";
			// var configKey = "llama3.2:1b";
			// modelName = hostBuilder.Configuration[configKey];
			modelName = "llama3.2:1b";
			if (string.IsNullOrEmpty(modelName))
			{
				// throw new InvalidOperationException($"No {nameof(modelName)} was specified, and none could be found from configuration at '{configKey}'");
				throw new InvalidOperationException($"No {nameof(modelName)} was specified.");
			}
		}

		if (hostBuilder.Configuration.GetConnectionString("ollama") is string ollamaEndpoint && !string.IsNullOrWhiteSpace(ollamaEndpoint))
		{
			return hostBuilder.Services.AddOllamaChatClient(
				modelName,
				new Uri(ollamaEndpoint.Split("=").Last()),
				builder);
		}
		else
		{
			throw new InvalidOperationException("Couldn't register AddOllamaChatClient.");
		}
	}

	public static IServiceCollection AddOllamaChatClient(
		this IServiceCollection services,
		string modelName,
		Uri? uri = null,
		Func<ChatClientBuilder, ChatClientBuilder>? builder = null)
	{
		uri ??= new Uri("http://localhost:11434");
		return services.AddChatClient(pipeline =>
		{
			builder?.Invoke(pipeline);

			// Temporary workaround for Ollama issues
			pipeline.UsePreventStreamingWithFunctions();

			var httpClient = pipeline.Services.GetService<HttpClient>() ?? new();
			return pipeline.Use(new OllamaChatClient(uri, modelName, httpClient));
		});
	}
}

/// <summary>
/// Ref: https://github.com/dotnet/eShopSupport/blob/main/src/ServiceDefaults/Clients/ChatCompletion/ServiceCollectionChatClientExtensions.cs
/// This is used only with Ollama, because the current version of Ollama doesn't support streaming with tool calls.
/// To work around this Ollama limitation, if the call involves tools, we always resolve it using the non-streaming endpoint.
/// </summary>
public static class PreventStreamingWithFunctionsExtensions
{
	public static ChatClientBuilder UsePreventStreamingWithFunctions(this ChatClientBuilder builder)
	{
		return builder.Use(inner => new PreventStreamingWithFunctions(inner));
	}

	private class PreventStreamingWithFunctions(IChatClient innerClient) : DelegatingChatClient(innerClient)
	{
		public override Task<ChatCompletion> CompleteAsync(IList<ChatMessage> chatMessages, ChatOptions? options = null, CancellationToken cancellationToken = default)
		{
			// Temporary workaround for an issue in CompleteAsync<T>. Although OpenAI models are happy to
			// receive system messages at the end of the conversation, it causes a lot of problems for
			// Llama 3. So replace the schema prompt role with User. We'll update CompleteAsync<T> to
			// do this natively in the next update.
			if (chatMessages.Count > 1
				&& chatMessages.LastOrDefault() is { } lastMessage
				&& lastMessage.Role == ChatRole.System
				&& lastMessage.Text?.Contains("$schema") is true)
			{
				lastMessage.Role = ChatRole.User;
			}

			return base.CompleteAsync(chatMessages, options, cancellationToken);
		}

		public override IAsyncEnumerable<StreamingChatCompletionUpdate> CompleteStreamingAsync(IList<ChatMessage> chatMessages, ChatOptions? options = null, CancellationToken cancellationToken = default)
		{
			return options?.Tools is null or []
				? base.CompleteStreamingAsync(chatMessages, options, cancellationToken)
				: TreatNonstreamingAsStreaming(chatMessages, options, cancellationToken);
		}

		private async IAsyncEnumerable<StreamingChatCompletionUpdate> TreatNonstreamingAsStreaming(IList<ChatMessage> chatMessages, ChatOptions options, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			var result = await CompleteAsync(chatMessages, options, cancellationToken);
			for (var choiceIndex = 0; choiceIndex < result.Choices.Count; choiceIndex++)
			{
				var choice = result.Choices[choiceIndex];
				yield return new StreamingChatCompletionUpdate
				{
					AuthorName = choice.AuthorName,
					ChoiceIndex = choiceIndex,
					CompletionId = result.CompletionId,
					Contents = choice.Contents,
					CreatedAt = result.CreatedAt,
					FinishReason = result.FinishReason,
					RawRepresentation = choice.RawRepresentation,
					Role = choice.Role,
					AdditionalProperties = result.AdditionalProperties,
				};
			}
		}
	}
}
