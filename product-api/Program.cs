// dotnet ef migrations add InitDb -c ProductDbContext -o Infrastructure/Migrations

using CoffeeShop.Shared.EF;
using CoffeeShop.Shared.Endpoint;
using CoffeeShop.Shared.Exceptions;
using CoffeeShop.Shared.OpenTelemetry;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;

using OpenAI;

using ProductApi.Infrastructure;
using ProductApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

//ef
builder.Services.AddDbContextPool<ProductDbContext>(dbContextOptionsBuilder =>
{
	dbContextOptionsBuilder.UseNpgsql(
		builder.Configuration.GetConnectionString("postgres"), builder =>
		{
			builder.UseVector();
		})
		.UseSnakeCaseNamingConvention();
});
builder.EnrichNpgsqlDbContext<ProductDbContext>();

builder.Services.AddMigration<ProductDbContext, ProductDbContextSeeder>();

//ai
if (builder.Configuration.GetValue<string>("ai:Type") is string type && type is "ollama")
{
	builder.Services.AddEmbeddingGenerator<string, Embedding<float>>(b => b
		.UseOpenTelemetry()
		.UseLogging()
		.Use(new OllamaEmbeddingGenerator(
			new Uri(builder.Configuration["AI:OLLAMA:Endpoint"]!),
			"all-minilm")));
}
else
{
	builder.AddAzureOpenAIClient("openai");
	builder.Services.AddEmbeddingGenerator<string, Embedding<float>>(b => b
		.UseOpenTelemetry()
		.UseLogging()
		.Use(b.Services.GetRequiredService<OpenAIClient>().AsEmbeddingGenerator(builder.Configuration.GetValue<string>("ai:EMBEDDINGMODEL")!)));
}

builder.Services.AddScoped<IProductItemAI, ProductItemAI>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddMediatR(cfg => {
	cfg.RegisterServicesFromAssemblyContaining<Program>();
	cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
	cfg.AddOpenBehavior(typeof(HandlerBehavior<,>));
});
builder.Services.AddValidatorsFromAssemblyContaining<Program>(includeInternalTypes: true);

builder.Services.AddCors();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApiVersioning(options =>
{
	options.DefaultApiVersion = new ApiVersion(1);
	options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
	options.GroupNameFormat = "'v'V";
	options.SubstituteApiVersionInUrl = true;
}).EnableApiVersionBinding();

builder.Services.AddEndpoints(typeof(Program).Assembly);

builder.Services.AddSingleton<IActivityScope, ActivityScope>();
builder.Services.AddSingleton<CommandHandlerMetrics>();
builder.Services.AddSingleton<QueryHandlerMetrics>();
builder.AddChatCompletionService("openai");

var app = builder.Build();

var apiVersionSet = app.NewApiVersionSet()
	.HasApiVersion(new ApiVersion(1))
	.ReportApiVersions()
	.Build();

var versionedGroup = app
	.MapGroup("api/v{version:apiVersion}")
	.WithApiVersionSet(apiVersionSet);

var apiVersionSetV2 = app.NewApiVersionSet()
	.HasApiVersion(new ApiVersion(2))
	.ReportApiVersions()
	.Build();

var versionedGroupV2 = app
	.MapGroup("api/v2")
	.WithApiVersionSet(apiVersionSetV2);

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseRouting();

app.MapDefaultEndpoints();
app.MapEndpoints(versionedGroup);
app.MapEndpoints(versionedGroupV2);

app.Run();

public partial class Program;
