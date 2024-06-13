using CounterApi.IntegrationEvents.EventHandlers;
using CounterApi.Infrastructure.Gateways;
using CounterApi.Domain;
using CoffeeShop.Shared.Endpoint;
using CoffeeShop.Shared.Exceptions;
using CoffeeShop.Shared.OpenTelemetry;
using CoffeeShop.Shared.OpenTelemetry.OtelMassTransit;
using System.Diagnostics.CodeAnalysis;

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

builder.Services.AddSingleton<IActivityScope, ActivityScope>();
builder.Services.AddSingleton<CommandHandlerMetrics>();
builder.Services.AddSingleton<QueryHandlerMetrics>();
builder.Services.AddScoped<IItemGateway, ItemHttpGateway>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<BaristaOrderUpdatedConsumer>(typeof(BaristaOrderUpdatedConsumerDefinition));
    x.AddConsumer<KitchenOrderUpdatedConsumer>(typeof(KitchenOrderUpdatedConsumerDefinition));

    x.SetKebabCaseEndpointNameFormatter();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq")!);

		cfg.UseSendFilter(typeof(OtelSendFilter<>), context);
		cfg.UsePublishFilter(typeof(OtelPublishFilter<>), context);
		cfg.UseConsumeFilter(typeof(OTelConsumeFilter<>), context);

		cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

var apiVersionSet = app.NewApiVersionSet()
	.HasApiVersion(new ApiVersion(1))
	.ReportApiVersions()
	.Build();

var versionedGroup = app
	.MapGroup("api/v{version:apiVersion}")
	.WithApiVersionSet(apiVersionSet);

app.UseExceptionHandler();

if(app.Environment.IsDevelopment())
{
	app.UseSwagger();
}

app.UseRouting();

app.MapDefaultEndpoints();
app.MapEndpoints(versionedGroup);

app.Run();

[ExcludeFromCodeCoverage]
public partial class Program;
