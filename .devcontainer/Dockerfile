FROM mcr.microsoft.com/dotnet/sdk:8.0.100-jammy

# Install make
RUN apt update && apt install -y make

## https://github.com/dotnet/aspire/blob/main/docs/using-latest-daily.md
RUN dotnet workload update --skip-sign-check --source https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet8/nuget/v3/index.json \
    && dotnet workload install aspire --skip-sign-check --source https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet8/nuget/v3/index.json

# RUN dotnet workload install aspire
