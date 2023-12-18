include .env
export

run:
	dotnet run --project app-host/app-host.csproj
.PHONY: run

build:
	dotnet build coffeeshop-aspire.sln
.PHONY: build
