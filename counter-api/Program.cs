using System.Text.Json;
using FluentValidation;

using CounterApi.Domain;
using CounterApi.Infrastructure.Gateways;
using CounterApi.UseCases;
using MediatR;
using CounterApi.Domain.Messages;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

// todo
// builder.Services.AddDaprWorkflowClient();
// builder.Services.AddDaprWorkflow(options =>
// {
//     options.RegisterWorkflow<PlaceOrderWorkflow>();

//     options.RegisterActivity<NotifyActivity>();
//     options.RegisterActivity<AddOrderActivity>();
//     options.RegisterActivity<UpdateOrderActivity>();
// });

builder.Services.AddHttpContextAccessor();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// todo
// builder.Services.AddDaprClient();
// builder.Services.AddSingleton(new JsonSerializerOptions()
// {
//     PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
//     PropertyNameCaseInsensitive = true,
// });

// todo
// builder.Services.AddScoped<IItemGateway, ItemDaprGateway>();

// https://github.com/dapr/dotnet-sdk/blob/master/examples/Workflow/WorkflowConsoleApp/Program.cs#L31
// if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DAPR_GRPC_PORT")))
// {
//     Environment.SetEnvironmentVariable("DAPR_GRPC_PORT", "50001");
// }

// builder.AddOpenTelemetry();

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

// todo
_ = app.MapOrderInApiRoutes()
    .MapOrderUpApiRoutes()
    .MapOrderFulfillmentApiRoutes();

app.Run();
