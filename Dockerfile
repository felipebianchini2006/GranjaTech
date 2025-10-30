# ==============================================
# Dockerfile para GranjaTech Backend (.NET 8)
# ==============================================

# Estágio 1: Runtime base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Estágio 2: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia arquivos de projeto para restauração eficiente (cache de camadas)
COPY ["GranjaTech.sln", "./"]
COPY ["GranjaTech.Domain/GranjaTech.Domain.csproj", "GranjaTech.Domain/"]
COPY ["GranjaTech.Infrastructure/GranjaTech.Infrastructure.csproj", "GranjaTech.Infrastructure/"]
COPY ["GranjaTech.Application/GranjaTech.Application.csproj", "GranjaTech.Application/"]
COPY ["GranjaTech.Api/GranjaTech.Api.csproj", "GranjaTech.Api/"]

# Restore de dependências
RUN dotnet restore "GranjaTech.sln"

# Copia todo o código fonte
COPY . .

# Build do projeto
WORKDIR "/src/GranjaTech.Api"
RUN dotnet build "GranjaTech.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Estágio 3: Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "GranjaTech.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# Estágio 4: Final runtime
FROM base AS final
WORKDIR /app

# Copia os binários publicados
COPY --from=publish /app/publish .

# Healthcheck
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Usuario não-root para segurança
USER $APP_UID

ENTRYPOINT ["dotnet", "GranjaTech.Api.dll"]
