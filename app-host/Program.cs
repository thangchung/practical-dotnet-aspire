var builder = DistributedApplication.CreateBuilder(args);

var postgresQL = builder.AddPostgres("postgresQL").WithPgAdmin();
var postgres = postgresQL.AddDatabase("postgres");

var redis = builder.AddRedis("redis");
var rabbitmq = builder.AddRabbitMQ("rabbitmq").WithManagementPlugin();

var productApi = builder.AddProject<Projects.CoffeeShop_ProductApi>("product-api");

builder.AddProject<Projects.CoffeeShop_CounterApi>("counter-api")
	.WithReference(productApi)
	.WithReference(rabbitmq);

builder.AddProject<Projects.CoffeeShop_BaristaApi>("barista-api")
	.WithReference(rabbitmq);

builder.AddProject<Projects.CoffeeShop_KitchenApi>("kitchen-api")
	.WithReference(rabbitmq);

builder.AddProject<Projects.CoffeeShop_OrderSummary>("order-summary")
	.WithReference(postgres)
	.WithReference(rabbitmq);

builder.Build().Run();
