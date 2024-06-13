using CoffeeShop.OrderSummary.Consumers;
using CoffeeShop.OrderSummary.Features;
using CoffeeShop.OrderSummary.Models;
using CoffeeShop.Shared.Endpoint;
using CoffeeShop.Shared.Exceptions;
using CoffeeShop.Shared.OpenTelemetry;

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

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApiVersioning(options =>
{
	options.DefaultApiVersion = new ApiVersion(1);
	options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
	options.GroupNameFormat = "'v'V";
	options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddEndpoints(typeof(Program).Assembly);

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

builder.Services.AddSingleton<IActivityScope, ActivityScope>();
builder.Services.AddSingleton<CommandHandlerMetrics>();
builder.Services.AddSingleton<QueryHandlerMetrics>();

var app = builder.Build();

var apiVersionSet = app.NewApiVersionSet()
	.HasApiVersion(new ApiVersion(1))
	.ReportApiVersions()
	.Build();

var versionedGroup = app
	.MapGroup("api/v{version:apiVersion}")
	.WithApiVersionSet(apiVersionSet);

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
}

app.UseRouting();

app.MapDefaultEndpoints();

app.MapEndpoints(versionedGroup);

app.Run();

public partial class Program;


