using Aspirant.Hosting;

using CoffeeShop.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var postgresQL = builder.AddPostgres("postgresQL").WithHealthCheck().WithPgAdmin();
var postgres = postgresQL.AddDatabase("postgres");

var redis = builder.AddRedis("redis").WithHealthCheck();
var rabbitmq = builder.AddRabbitMQ("rabbitmq").WithHealthCheck().WithManagementPlugin();

var productApi = builder.AddProject<Projects.CoffeeShop_ProductApi>("product-api")
	.WithSwaggerUI();

var counterApi = builder.AddProject<Projects.CoffeeShop_CounterApi>("counter-api")
	.WithReference(productApi)
	.WithReference(rabbitmq)
	.WaitFor(rabbitmq)
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

var isHttps = builder.Configuration["DOTNET_LAUNCH_PROFILE"] == "https";
var ingressPort = int.TryParse(builder.Configuration["Ingress:Port"], out var port) ? port : (int?)null;

builder.AddYarp("ingress")
	.WithEndpoint(scheme: isHttps ? "https" : "http", port: ingressPort)
	.WithReference(productApi)
	.WithReference(counterApi)
	.WithReference(orderSummaryApi)
	.LoadFromConfiguration("ReverseProxy");

builder.Build().Run();
