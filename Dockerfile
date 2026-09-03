# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy Solution and Project files for efficient Docker layer caching
COPY ["PortFolio.sln", "./"]
COPY ["Web/Web.csproj", "Web/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["ApplicationCore/ApplicationCore.csproj", "ApplicationCore/"]
COPY ["InFrastructure/InFrastructure.csproj", "InFrastructure/"]
COPY ["CarRental.Tests/CarRental.Tests.csproj", "CarRental.Tests/"]

# Restore packages
RUN dotnet restore "PortFolio.sln"

# Copy full source and publish
COPY . .
WORKDIR "/src/Web"
RUN dotnet publish "Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render dynamically sets PORT env variable (defaults to 8080)
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Web.dll"]
