# IoBuild.Projects - Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/IoBuild.Shared/IoBuild.Shared.csproj", "src/IoBuild.Shared/"]
COPY ["src/IoBuild.Projects/IoBuild.Projects.csproj", "src/IoBuild.Projects/"]
RUN dotnet restore "src/IoBuild.Projects/IoBuild.Projects.csproj"
COPY . .
RUN dotnet build "src/IoBuild.Projects/IoBuild.Projects.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/IoBuild.Projects/IoBuild.Projects.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
ENV ASPNETCORE_URLS=http://+:5003
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=publish /app/publish .
EXPOSE 5003
ENTRYPOINT ["dotnet", "IoBuild.Projects.dll"]
