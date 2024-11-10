using CoffeeShop.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var postgresQL = builder.AddPostgres("postgresQL")
						.WithImage("ankane/pgvector")
						.WithImageTag("latest")
						.WithLifetime(ContainerLifetime.Persistent)
						.WithHealthCheck()
						.WithPgAdmin();
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

var productApi = builder.AddProject<Projects.CoffeeShop_ProductApi>("product-api")
						.WithReference(postgres).WaitFor(postgres)
						.WithSwaggerUI();

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

var orderSummaryApi = builder.AddProject<Projects.CoffeeShop_OrderSummary>("order-summary")
							.WithReference(postgres)
							.WithReference(rabbitmq)
							.WaitFor(postgres)
							.WaitFor(rabbitmq)
							.WithSwaggerUI();

builder.AddProject<Projects.CoffeeShop_Yarp>("yarp")
	.WithReference(productApi)
	.WithReference(counterApi)
	.WithReference(orderSummaryApi)
	.WaitFor(productApi)
	.WaitFor(counterApi)
	.WaitFor(orderSummaryApi);

builder.Build().Run();
