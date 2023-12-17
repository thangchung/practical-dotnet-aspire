var builder = DistributedApplication.CreateBuilder(args);

// var rabbitmq = builder.AddRabbitMQContainer("rabbitmq");

builder.AddProject<Projects.product_api>("productapi");

builder.AddProject<Projects.counter_api>("counterapi");

builder.AddProject<Projects.barista_api>("baristaapi");

builder.AddProject<Projects.kitchen_api>("kitchenapi");

builder.Build().Run();
