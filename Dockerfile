FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["AgriYield.Api/AgriYield.Api.csproj", "AgriYield.Api/"]
COPY ["AgriYield.Domain/AgriYield.Domain.csproj", "AgriYield.Domain/"]
COPY ["AgriYield.Infrastructure/AgriYield.Infrastructure.csproj", "AgriYield.Infrastructure/"]

RUN dotnet restore "./AgriYield.Api/AgriYield.Api.csproj"
COPY . .
WORKDIR "/src/AgriYield.Api"
RUN dotnet build "./AgriYield.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AgriYield.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AgriYield.Api.dll"]
