# Multi-stage Dockerfile for ASISYA .NET 8 Web API
# Stage 1: Build and test
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for granular dependency layer caching
COPY Asisya.sln ./
COPY src/Asisya.Domain/Asisya.Domain.csproj src/Asisya.Domain/
COPY src/Asisya.Application/Asisya.Application.csproj src/Asisya.Application/
COPY src/Asisya.Infrastructure/Asisya.Infrastructure.csproj src/Asisya.Infrastructure/
COPY src/Asisya.WebApi/Asisya.WebApi.csproj src/Asisya.WebApi/
COPY tests/Asisya.Application.Tests/Asisya.Application.Tests.csproj tests/Asisya.Application.Tests/
COPY tests/Asisya.IntegrationTests/Asisya.IntegrationTests.csproj tests/Asisya.IntegrationTests/

# Restore packages
RUN dotnet restore Asisya.sln

# Copy all repository source files
COPY . .

# Execute unit test suite to enforce quality gate prior to artifact publishing
RUN dotnet restore tests/Asisya.Application.Tests/Asisya.Application.Tests.csproj --force
RUN dotnet test tests/Asisya.Application.Tests/Asisya.Application.Tests.csproj -c Release --no-restore

# Publish the Web API
WORKDIR /src/src/Asisya.WebApi
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Production runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Secure non-root unprivileged container execution
USER app

EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Development

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Asisya.WebApi.dll"]
