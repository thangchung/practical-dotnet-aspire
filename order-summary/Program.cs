using CoffeeShop.OrderSummary;
using CoffeeShop.OrderSummary.Consumers;

using FluentValidation;

using JasperFx.CodeGeneration;

using Marten;
using Marten.AspNetCore;
using Marten.Events.Daemon.Resiliency;
using Marten.Events.Projections;

using MassTransit;

using Weasel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

builder.Services.AddHttpContextAccessor();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddMassTransit(x =>
{
	x.AddConsumer<OrderConsumer>();

	x.SetKebabCaseEndpointNameFormatter();

	x.UsingRabbitMq((context, cfg) =>
	{
		cfg.Host(builder.Configuration.GetConnectionString("rabbitmq")!);
		cfg.ConfigureEndpoints(context);
	});
});

builder.Services.AddMarten(sp =>
{
	var options = new StoreOptions();

	var schemaName = Environment.GetEnvironmentVariable("SchemaName") ?? "order_summary";
	options.Events.DatabaseSchemaName = schemaName;
	options.DatabaseSchemaName = schemaName;
	options.Connection(builder.Configuration.GetConnectionString("postgres") ??
					   throw new InvalidOperationException());

	options.UseSystemTextJsonForSerialization(EnumStorage.AsString);

	options.Projections.Errors.SkipApplyErrors = false;
	options.Projections.Errors.SkipSerializationErrors = false;
	options.Projections.Errors.SkipUnknownEvents = false;

	options.Projections.LiveStreamAggregation<Order>();
	options.Projections.Add<OrderSummaryProjection>(ProjectionLifecycle.Async);

	return options;
})
.OptimizeArtifactWorkflow(TypeLoadMode.Static)
.UseLightweightSessions()
.AddAsyncDaemon(DaemonMode.Solo);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler();
}
else
{
	app.UseSwagger();
}

app.UseRouting();

app.MapDefaultEndpoints();

app.MapGet("summary", (HttpContext context, IQuerySession querySession, Guid orderId) =>
	querySession.Json.WriteById<OrderSummary>(orderId, context)
);

app.Run();

public partial class Program;
