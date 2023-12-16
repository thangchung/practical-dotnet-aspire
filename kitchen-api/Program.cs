using System.Text.Json;
using FluentValidation;

using KitchenApi.UseCases;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

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

_ = app.MapOrderUpApiRoutes();

app.Run();
