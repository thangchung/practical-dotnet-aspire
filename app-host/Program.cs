using CoffeeShop.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var postgresQL = builder.AddPostgres("postgresQL")
						.WithImage("ankane/pgvector")
						.WithImageTag("latest")
						.WithLifetime(ContainerLifetime.Persistent)
						.WithHealthCheck()
						.WithPgWeb();
var postgres = postgresQL.AddDatabase("postgres");

var redis = builder.AddRedis("redis")
					// .WithContainerName("redis") // use an existing container
					.WithLifetime(ContainerLifetime.Persistent)
					.WithHealthCheck()
					.WithRedisCommander();

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
						.WithLifetime(ContainerLifetime.Persistent)	
						.WithHealthCheck()
						.WithManagementPlugin();

var ollama = builder.AddOllama("ollama")
				.WithImageTag("0.5.7")
				.WithLifetime(ContainerLifetime.Persistent)
				.WithDataVolume()
				//.WithGPUSupport()
				.WithOpenWebUI()
				;

var allMinilmModel = ollama.AddModel("embedding", "all-minilm");
// var llama32Model = ollama.AddModel("llama32", "llama3.2:1b");
var deepseekModel = ollama.AddModel("chat", "deepseek-r1:1.5b");

var productApi = builder.AddProject<Projects.CoffeeShop_ProductApi>("product-api")
						.WithReference(postgres).WaitFor(postgres)
						.WithEnvironment("AI:Type", "ollama")
						.WithEnvironment("AI:EMBEDDINGMODEL", "all-minilm")
						.WithEnvironment("AI:CHATMODEL", "deepseek-r1:1.5b")
						.WithReference(ollama).WaitFor(allMinilmModel).WaitFor(deepseekModel)
						.WithSwaggerUI();

// set to true if you want to use OpenAI
bool useOpenAI = false;
if (useOpenAI)
{
	var openAI = builder.AddConnectionString("openai");
	productApi
			.WithReference(openAI)
			.WithEnvironment("AI:Type", "openai")
			.WithEnvironment("AI:EMBEDDINGMODEL", "text-embedding-3-small")
			.WithEnvironment("AI:CHATMODEL", "gpt-4o-mini");
}

var counterApi = builder.AddProject<Projects.CoffeeShop_CounterApi>("counter-api")
						.WithReference(productApi)
						.WithReference(rabbitmq).WaitFor(rabbitmq)
						.WithSwaggerUI();

builder.AddProject<Projects.CoffeeShop_BaristaApi>("barista-api")
		.WithReference(rabbitmq)
		.WaitFor(rabbitmq);

builder.AddProject<Projects.CoffeeShop_KitchenApi>("kitchen-api")
		.WithReference(rabbitmq)
		.WaitFor(rabbitmq);

//var orderSummaryApi = builder.AddProject<Projects.CoffeeShop_OrderSummary>("order-summary")
//							.WithReference(postgres)
//							.WithReference(rabbitmq)
//							.WaitFor(postgres)
//							.WaitFor(rabbitmq)
//							.WithSwaggerUI();

builder.AddProject<Projects.CoffeeShop_Yarp>("yarp")
	.WithReference(productApi).WaitFor(productApi)
	.WithReference(counterApi).WaitFor(counterApi)
	// .WithReference(orderSummaryApi).WaitFor(orderSummaryApi)
	;

builder.Build().Run();
