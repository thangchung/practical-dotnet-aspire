# The Coffeeshop Apps on .NET Aspire

The coffeeshop apps on .NET Aspire

![Counter API-Code Coverage](https://img.shields.io/badge/Code%20Coverage-73%25-yellow?style=flat)

## Introduction

> Notice: This is just a demo of how can we build and deploy the microservices approach. In the reality, the boundary should be defined by bounded-context concepts of Domaim-driven Design, and totally based on the business domain, and might not be so fine-grained services like this demo, so you use it with care.

- [x] Built on .NET 8.0 LTS
- [x] Microservices architectural style
- [x] Follows Vertical Sliding principles
- [x] Domain Driven Design building blocks
- [x] CQRS with MediatR and Fluent Validations
- [x] Shift-left Observability with .NET Aspire (OpenTelemetry built-in)
	- [x] Custom OpenTelemetry for MediatR and FluentValidation handlers
	- [x] Custom OpenTelemetry for MassTransit on consumers
	- [x] Enrich .NET 8 global loggings
- [x] OpenAPI supports
- [x] Mapperly for generating object mappings
- [x] API Versioning
- [x] Integration test with .NET Aspire and Wiremock.NET
	- [x] Run it on GitHub Actions and output code coverage
- [ ] Response Caching - Distributed Caching with Redis
- [ ] Dapr integration
- [ ] JWT & Authentication with Keycloak

## System Context diagram - C4 Model

```mermaid
C4Context
	title System Context diagram for CoffeeShop Application
	Boundary(b0, "Boundary1") {
		Person(customer, "Customers", "Customers of the coffeeshop.")

		Boundary(b1, "Application", "boundary") {
			System(SystemA, "CoffeeShop app", "Allows customers to submit and view their orders.")
		}

		Boundary(b2, "Infrastructure", "boundary") {
			SystemDb(SystemD, "Database", "A system of the coffeeshop app.")
			SystemQueue(SystemQ, "Message Queue", "A system of the coffeeshop app.")
		}
	}

	Rel(customer, SystemA, "Uses")
	Rel(SystemA, SystemD, "Uses")
	Rel(SystemA, SystemQ, "Uses")
```

## Container diagram - C4 Model

```mermaid
C4Container
	title Container diagram for CoffeeShop Application

	Person(customer, "Customers", "Customers of the coffeeshop.")

	Container_Boundary(c1, "CoffeeShop Application") {
		Container(reverse_proxy, "Gateway", "C#, .NET 8, YARP", "The reverse proxy/API gateway of the coffeeshop app.")

		Container(counter_api, "Counter APIs", "C#, .NET 8, MassTransit", "The counter service.")
		Container(barista_api, "Barista APIs", "C#, .NET 8, MassTransit", "The barista service.")
		Container(kitchen_api, "Kitchen APIs", "C#, .NET 8, MassTransit", "The kitchen service.")
		Container(order_summary, "Order Summary", "C#, .NET 8, Marten", "The order summary service.")

		Container(product_api, "Product APIs", "C#, .NET 8", "The product service.")
		
		Boundary(b1, "Docker containers", "boundary") {
			ContainerDb(database, "Database", "Postgres", "Stores orders, audit logs, etc.")
			ContainerQueue(message_broker, "Message Broker", "RabbitMQ", "Asynchronous communication between counter, barista, kitchen, and order-summary")
		}
	}

	Rel(customer, reverse_proxy, "Uses", "HTTPS")
	
	Rel(reverse_proxy, product_api, "Proxies", "HTTP")
	Rel(reverse_proxy, counter_api, "Proxies", "HTTP")

	Rel(order_summary, database, "Uses", "TCP")
	
	Rel(counter_api, product_api, "Calls", "HTTP")
	Rel(counter_api, message_broker, "Publishes", "TCP")
	Rel(counter_api, message_broker, "Publishes", "TCP")
	
	Rel_Back(barista_api, message_broker, "Subscribes", "TCP")
	Rel_Back(kitchen_api, message_broker, "Subscribes", "TCP")
	Rel_Back(order_summary, message_broker, "Subscribes", "TCP")
```

## Prerequisites

If you run on `Windows 11`: 

```bash
> cargo install just
# https://cheatography.com/linux-china/cheat-sheets/justfile/
```

## Get starting

```sh
> dotnet build coffeeshop-aspire.sln
> dotnet run --project app-host/app-host.csproj
# http://localhost:5019
```

## Generate manifest file (powershell)

```sh
dotnet run --project app-host\CoffeeShop.AppHost.csproj `
    --publisher manifest `
    --output-path ../aspire-manifest.json
```

## Deploy to Kubernetes

```sh
dotnet tool install -g aspirate --prerelease
```

## Run with Justfile (cross-platform)

```sh
> just run
```

On Windows 11 - WSL2 Ubuntu 22 integrated, we can use `Podman Desktop` to replace `Docker for Desktop`, and run `.NET Aspire` normally. Check this blog post -> https://dev.to/thangchung/net-8-integration-tests-on-podman-desktop-windows-11-wsl2-ubuntu-23-4hpo

## Run with Makefile (Ubuntu)

```sh
> touch .env
> make run
# http://localhost:5019
```

```sh
dotnet publish "/workspaces/coffeeshop-aspire/app-host/../product-api/CoffeeShop.ProductApi.csproj" -p:PublishProfile="DefaultContainer" -p:PublishSingleFile="true" 
-p:PublishTrimmed="false" --self-contained "true" --verbosity "quiet" --nologo -r "linux-x64" -p:ContainerRegistry="k3d-myregistry.localhost:12345" -p:ContainerRepository="product-api" -p:ContainerImageTag="latest"
```
