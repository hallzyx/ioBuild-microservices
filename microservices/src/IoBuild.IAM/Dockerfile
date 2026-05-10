# IoBuild.IAM - Dockerfile
# Multi-stage build para .NET 9

# ── Stage 1: Build ──
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restaurar paquetes (usa cache de capas)
COPY ["src/IoBuild.Shared/IoBuild.Shared.csproj", "src/IoBuild.Shared/"]
COPY ["src/IoBuild.IAM/IoBuild.IAM.csproj", "src/IoBuild.IAM/"]
RUN dotnet restore "src/IoBuild.IAM/IoBuild.IAM.csproj"

# Copiar todo y compilar
COPY . .
RUN dotnet build "src/IoBuild.IAM/IoBuild.IAM.csproj" -c Release -o /app/build

# ── Stage 2: Publish ──
FROM build AS publish
RUN dotnet publish "src/IoBuild.IAM/IoBuild.IAM.csproj" -c Release -o /app/publish --no-restore

# ── Stage 3: Runtime ──
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Instalar curl para health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Variable de entorno para forzar HTTP (los microservicios internos no usan HTTPS)
ENV ASPNETCORE_URLS=http://+:5001
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=publish /app/publish .

EXPOSE 5001

ENTRYPOINT ["dotnet", "IoBuild.IAM.dll"]
