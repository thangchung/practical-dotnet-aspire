using System.ClientModel;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace ProductApi.Services;

public static class ChatCompletionServiceExtensions
{
	public static void AddChatCompletionService(this IHostApplicationBuilder builder)
	{
		var pipeline = (ChatClientBuilder pipeline) => pipeline
			.UseFunctionInvocation()
			.UseOpenTelemetry(configure: c => c.EnableSensitiveData = true);

		if (builder.Configuration["AI:Type"] == "openai")
		{
			builder.AddOpenAIChatClient(builder: pipeline);
		}
		else
		{
			builder.AddOllamaChatClient(pipeline);
		}
	}

	public static IServiceCollection AddOllamaChatClient(
		this IHostApplicationBuilder hostBuilder,
		Func<ChatClientBuilder, ChatClientBuilder>? builder = null,
		string? modelName = null)
	{
		if (modelName is null)
		{
			modelName = hostBuilder.Configuration["AI:CHATMODEL"];
			if (string.IsNullOrEmpty(modelName))
			{
				throw new InvalidOperationException($"No {nameof(modelName)} was specified.");
			}
		}

		if (hostBuilder.Configuration.GetValue<string>("AI:Type") is string type && type is "ollama")
		{
			var connectionString = hostBuilder.Configuration.GetConnectionString(type);
			if (string.IsNullOrWhiteSpace(connectionString))
			{
				throw new InvalidOperationException($"No connection string named '{type}' was found. Ensure a corresponding Aspire service was registered.");
			}
			var connectionStringBuilder = new DbConnectionStringBuilder
			{
				ConnectionString = connectionString
			};
			var endpoint = (string?)connectionStringBuilder["endpoint"];

			return hostBuilder.Services.AddOllamaChatClient(
				modelName,
				new Uri(endpoint!),
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

	public static IServiceCollection AddOpenAIChatClient(
		this IHostApplicationBuilder hostBuilder,
		string serviceName = "openai",
		Func<ChatClientBuilder, ChatClientBuilder>? builder = null,
		string? modelOrDeploymentName = null)
	{
		// TODO: We would prefer to use Aspire.AI.OpenAI here, but it doesn't yet support the OpenAI v2 client.
		// So for now we access the connection string and set up a client manually.

		var connectionString = hostBuilder.Configuration.GetConnectionString(serviceName);
		if (string.IsNullOrWhiteSpace(connectionString))
		{
			throw new InvalidOperationException($"No connection string named '{serviceName}' was found. Ensure a corresponding Aspire service was registered.");
		}

		var connectionStringBuilder = new DbConnectionStringBuilder
		{
			ConnectionString = connectionString
		};
		var endpoint = (string?)connectionStringBuilder["endpoint"];
		var apiKey = (string)connectionStringBuilder["key"] ?? throw new InvalidOperationException($"The connection string named '{serviceName}' does not specify a value for 'Key', but this is required.");

		modelOrDeploymentName ??= hostBuilder.Configuration["ai:CHATMODEL"]!;
		if (string.IsNullOrWhiteSpace(modelOrDeploymentName))
		{
			throw new InvalidOperationException($"The connection string named '{serviceName}' does not specify a value for 'Deployment' or 'Model', and no value was passed for {nameof(modelOrDeploymentName)}.");
		}

		var endpointUri = string.IsNullOrEmpty(endpoint) ? null : new Uri(endpoint);
		return hostBuilder.Services.AddOpenAIChatClient(apiKey, modelOrDeploymentName, endpointUri, builder);
	}

	public static IServiceCollection AddOpenAIChatClient(
		this IServiceCollection services,
		string apiKey,
		string modelOrDeploymentName,
		Uri? endpoint = null,
		Func<ChatClientBuilder, ChatClientBuilder>? builder = null)
	{
		return services
			.AddSingleton(_ => endpoint is null
				? new OpenAIClient(apiKey)
				: new AzureOpenAIClient(endpoint, new ApiKeyCredential(apiKey)))
			.AddChatClient(pipeline =>
			{
				builder?.Invoke(pipeline);
				var openAiClient = pipeline.Services.GetRequiredService<OpenAIClient>();
				return pipeline.Use(openAiClient.AsChatClient(modelOrDeploymentName));
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
