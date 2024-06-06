# The Coffeeshop Apps on .NET Aspire

The coffeeshop apps on .NET Aspire

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
    -- `
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
