set dotenv-load := true
#serv: 
#   echo "$DATABASE_ADDRESS from .env"

build:
  dotnet build coffeeshop-aspire.sln

run:
  dotnet run --project app-host/app-host.csproj