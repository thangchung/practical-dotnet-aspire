using FluentValidation;
using MassTransit;
using BaristaApi.IntegrationEvents.EventHandlers;
using CoffeeShop.Shared.OpenTelemetry;
using CoffeeShop.Shared.Exceptions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHttpContextAccessor();
builder.Services.AddMediatR(cfg => {
	cfg.RegisterServicesFromAssemblyContaining<Program>();
	cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
	cfg.AddOpenBehavior(typeof(HandlerBehavior<,>));
});
builder.Services.AddValidatorsFromAssemblyContaining<Program>(includeInternalTypes: true);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddMassTransit(x =>
{
	x.AddConsumer<BaristaOrderedConsumer>(typeof(BaristaOrderedConsumerDefinition));

	x.SetKebabCaseEndpointNameFormatter();

	x.UsingRabbitMq((context, cfg) =>
	{
		cfg.Host(builder.Configuration.GetConnectionString("rabbitmq")!);
		cfg.ConfigureEndpoints(context);
	});
});

builder.Services.AddSingleton<IActivityScope, ActivityScope>();
builder.Services.AddSingleton<CommandHandlerMetrics>();
builder.Services.AddSingleton<QueryHandlerMetrics>();

var app = builder.Build();

app.UseExceptionHandler();

app.UseRouting();

app.MapDefaultEndpoints();

app.Run();

public partial class Program;

