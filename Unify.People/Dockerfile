# =========================
# Base runtime image
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Create non-root user
RUN useradd -m appuser

# =========================
# Build image
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Restore dependencies
RUN dotnet restore "./Unify.People/Unify.People.csproj"

# Copy everything else
COPY . .
WORKDIR "/src/Unify.People"

# Build project
RUN dotnet build "./Unify.People.csproj" -c $BUILD_CONFIGURATION -o /app/build

# =========================
# Publish image
# =========================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Unify.People.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# =========================
# Final runtime image
# =========================
FROM base AS final
WORKDIR /app

# Copy published app
COPY --from=publish /app/publish .

# Ensure VisitorPass folder exists and assign to appuser
RUN mkdir -p /app/wwwroot/VisitorPass \
    && chown -R appuser:appuser /app

# Switch to non-root
USER appuser

ENTRYPOINT ["dotnet", "Unify.People.dll"]
