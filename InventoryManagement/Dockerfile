FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src    
ENV ASPNETCORE_URLS=http://+:8080

# fix sqlclient kerberos issue
RUN apt-get update && apt-get install -y \
    krb5-user \
    libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
# Copy csproj dulu
COPY ./InventoryManagement/InventoryManagement.csproj ./

# Restore dependencies
RUN dotnet restore ".\InventoryManagement.csproj" --disable-parallel /v:detailed

# Copy semua source code
COPY . .

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./InventoryManagement/InventoryManagement.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InventoryManagement.dll"]
