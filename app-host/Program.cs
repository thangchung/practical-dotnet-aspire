var builder = DistributedApplication.CreateBuilder(args);

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
	.WithDataVolume()
	.WithManagementPlugin();

var productApi = builder.AddProject<Projects.CoffeeShop_ProductApi>("product-api");

builder.AddProject<Projects.CoffeeShop_CounterApi>("counter-api")
    .WithReference(productApi)
    .WithReference(rabbitmq);

builder.AddProject<Projects.CoffeeShop_BaristaApi>("barista-api")
    .WithReference(rabbitmq);

builder.AddProject<Projects.CoffeeShop_KitchenApi>("kitchen-api")
    .WithReference(rabbitmq);

builder.Build().Run();
