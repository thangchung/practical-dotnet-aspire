using FluentValidation;
using MassTransit;
using BaristaApi.IntegrationEvents.EventHandlers;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

builder.Services.AddHttpContextAccessor();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

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

var app = builder.Build();

app.UseExceptionHandler();

app.UseRouting();

app.MapDefaultEndpoints();

app.Map("/", () => Results.Redirect("/swagger"));

// _ = app.MapOrderUpApiRoutes();

app.Run();
