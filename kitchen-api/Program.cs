using FluentValidation;
using KitchenApi.IntegrationEvents.EventHandlers;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

builder.Services.AddHttpContextAccessor();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<KitchenOrderedConsumer>(typeof(KitchenOrderedConsumerDefinition));

    x.SetKebabCaseEndpointNameFormatter();

    x.UsingRabbitMq((context, cfg) =>
    {
        // cfg.Host(builder.Configuration.GetValue<string>("RabbitMqUrl")!);

        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq")!);
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.MapDefaultEndpoints();

app.Map("/", () => Results.Redirect("/swagger"));

// _ = app.MapOrderUpApiRoutes();

app.Run();
