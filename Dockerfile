# ==========================================
# 1. BASE RUNTIME STAGE
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
# Render uses port 8080 by default for ASP.NET Core
EXPOSE 8080
EXPOSE 8081

# ==========================================
# 2. BUILD STAGE
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the project file and restore dependencies
# This is done separately to take advantage of Docker caching
COPY ["InventoryManager.csproj", "./"]
RUN dotnet restore "InventoryManager.csproj"

# Copy the rest of the application code
COPY . .

# Build the application
RUN dotnet build "InventoryManager.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ==========================================
# 3. PUBLISH STAGE
# ==========================================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "InventoryManager.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ==========================================
# 4. FINAL RUNTIME STAGE
# ==========================================
FROM base AS final
WORKDIR /app
# Copy the published output from the publish stage
COPY --from=publish /app/publish .

# Set the entry point to run your DLL
ENTRYPOINT ["dotnet", "InventoryManager.dll"]