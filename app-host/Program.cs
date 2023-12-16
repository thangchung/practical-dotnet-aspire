using System.Collections.Immutable;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// builder.Services.AddDataProtection()
// //     .DisableAutomaticKeyGeneration();
//     .PersistKeysToFileSystem(new DirectoryInfo(@"\\workspaces\coffeeshop-aspire"));

builder.AddProject<Projects.product_api>("productapi")
    .WithEnvironment("ASPNETCORE_URLS", "http://+:5001");

builder.AddProject<Projects.counter_api>("counterapi")
    .WithEnvironment("ASPNETCORE_URLS", "http://+:5002");

builder.AddProject<Projects.barista_api>("baristaapi")
    .WithEnvironment("ASPNETCORE_URLS", "http://+:5003");

builder.AddProject<Projects.kitchen_api>("kitchenapi")
    .WithEnvironment("ASPNETCORE_URLS", "http://+:5004");

builder.Build().Run();
