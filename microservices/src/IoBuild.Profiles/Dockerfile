# IoBuild.Profiles - Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/IoBuild.Shared/IoBuild.Shared.csproj", "src/IoBuild.Shared/"]
COPY ["src/IoBuild.Profiles/IoBuild.Profiles.csproj", "src/IoBuild.Profiles/"]
RUN dotnet restore "src/IoBuild.Profiles/IoBuild.Profiles.csproj"
COPY . .
RUN dotnet build "src/IoBuild.Profiles/IoBuild.Profiles.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/IoBuild.Profiles/IoBuild.Profiles.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
ENV ASPNETCORE_URLS=http://+:5007
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=publish /app/publish .
EXPOSE 5007
ENTRYPOINT ["dotnet", "IoBuild.Profiles.dll"]
