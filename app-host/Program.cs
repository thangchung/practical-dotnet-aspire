var builder = DistributedApplication.CreateBuilder(args);

var rabbitmq = builder.AddRabbitMQContainer("rabbitmq");

var productApi = builder.AddProject<Projects.product_api>("productapi")
    .WithReplicas(2);

builder.AddProject<Projects.counter_api>("counterapi")
    .WithReference(productApi)
    .WithReference(rabbitmq);

builder.AddProject<Projects.barista_api>("baristaapi")
    .WithReference(rabbitmq)
    .WithReplicas(2);

builder.AddProject<Projects.kitchen_api>("kitchenapi")
    .WithReference(rabbitmq)
    .WithReplicas(2);

builder.Build().Run();
