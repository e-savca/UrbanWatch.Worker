FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base

# link to repo
LABEL org.opencontainers.image.source https://github.com/e-savca/UrbanWatch.Worker

WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
WORKDIR /src/src/UrbanWatch.Worker
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UrbanWatch.Worker.dll"]