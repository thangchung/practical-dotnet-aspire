using FluentValidation;
using CounterApi.UseCases;
using MassTransit;
using CounterApi.IntegrationEvents.EventHandlers;
using CounterApi.Infrastructure.Gateways;
using CounterApi.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

builder.Services.AddHttpContextAccessor();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddEndpointsApiExplorer();

// builder.Services.AddHttpClient<ProductHttpClient>(client => 
//     client.BaseAddress = new(builder.Configuration.GetValue<string>("ProductApiUrl")!));
builder.Services.AddScoped<IItemGateway, ItemHttpGateway>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<BaristaOrderUpdatedConsumer>(typeof(BaristaOrderUpdatedConsumerDefinition));
    x.AddConsumer<KitchenOrderUpdatedConsumer>(typeof(KitchenOrderUpdatedConsumerDefinition));

    x.SetKebabCaseEndpointNameFormatter();

    x.UsingRabbitMq((context, cfg) =>
    {
        // Console.WriteLine($"RabbitMQ Conn: {builder.Configuration.GetConnectionString("rabbitmq")}");
        // cfg.Host(new Uri(builder.Configuration.GetConnectionString("rabbitmq")!), h => {
        //     h.Username("guest");
        //     h.Password("guest");
        // });
        
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq")!);
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.UseExceptionHandler();

app.UseRouting();

app.MapDefaultEndpoints();

app.Map("/", () => Results.Redirect("/swagger"));

// todo
_ = app.MapOrderInApiRoutes()
    // .MapOrderUpApiRoutes()
    .MapOrderFulfillmentApiRoutes();

app.Run();
