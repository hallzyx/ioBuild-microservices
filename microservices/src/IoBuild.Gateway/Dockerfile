# IoBuild.Gateway - Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/IoBuild.Shared/IoBuild.Shared.csproj", "src/IoBuild.Shared/"]
COPY ["src/IoBuild.Gateway/IoBuild.Gateway.csproj", "src/IoBuild.Gateway/"]
RUN dotnet restore "src/IoBuild.Gateway/IoBuild.Gateway.csproj"
COPY . .
RUN dotnet build "src/IoBuild.Gateway/IoBuild.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/IoBuild.Gateway/IoBuild.Gateway.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Gateway es el único que expone puerto público
# ASPNETCORE_ENVIRONMENT=Docker → carga appsettings.Docker.json para URLs internas
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Docker

COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "IoBuild.Gateway.dll"]
