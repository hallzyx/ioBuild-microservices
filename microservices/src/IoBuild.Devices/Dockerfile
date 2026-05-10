# IoBuild.Devices - Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/IoBuild.Shared/IoBuild.Shared.csproj", "src/IoBuild.Shared/"]
COPY ["src/IoBuild.Devices/IoBuild.Devices.csproj", "src/IoBuild.Devices/"]
RUN dotnet restore "src/IoBuild.Devices/IoBuild.Devices.csproj"
COPY . .
RUN dotnet build "src/IoBuild.Devices/IoBuild.Devices.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/IoBuild.Devices/IoBuild.Devices.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
ENV ASPNETCORE_URLS=http://+:5002
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=publish /app/publish .
EXPOSE 5002
ENTRYPOINT ["dotnet", "IoBuild.Devices.dll"]
