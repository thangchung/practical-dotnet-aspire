using CoffeeShop.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var postgresQL = builder.AddPostgres("postgresQL")
						.WithImage("ankane/pgvector")
						.WithImageTag("latest")
						.WithLifetime(ContainerLifetime.Persistent)
						.WithHealthCheck()
						.WithPgWeb()
						//.WithPgAdmin()
						;
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
				.WithImageTag("0.3.14")
				.WithLifetime(ContainerLifetime.Persistent)
				.WithDataVolume()
				//.WithOpenWebUI()
				;

var allMinilmModel = ollama.AddModel("all-minilm", "all-minilm");
var llama32Model = ollama.AddModel("llama32", "llama3.2:1b");

var productApi = builder.AddProject<Projects.CoffeeShop_ProductApi>("product-api")
						.WithReference(postgres).WaitFor(postgres)
						.WithEnvironment($"ai:Type", "ollama")
						.WithEnvironment($"ai:EMBEDDINGMODEL", "all-minilm")
						.WithEnvironment($"ai:CHATMODEL", "llama3.2:1b")
						.WithReference(ollama).WaitFor(allMinilmModel).WaitFor(llama32Model)
						.WithSwaggerUI();

// set to true if you want to use OpenAI
bool useOpenAI = false;
if (useOpenAI)
{
	var openAI = builder.AddConnectionString("openai");
	productApi
			.WithReference(openAI)
			.WithEnvironment("ai:Type", "openai")
			.WithEnvironment("ai:EMBEDDINGMODEL", "text-embedding-3-small")
			.WithEnvironment("ai:CHATMODEL", "gpt-4o-mini");
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
