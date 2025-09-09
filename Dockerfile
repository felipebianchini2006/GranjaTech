# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia solução e csprojs para cache eficiente
# Ajuste o nome da .sln se necessário
COPY GranjaTech.sln ./
COPY GranjaTech.Domain/GranjaTech.Domain.csproj GranjaTech.Domain/
COPY GranjaTech.Infrastructure/GranjaTech.Infrastructure.csproj GranjaTech.Infrastructure/
COPY GranjaTech.Application/GranjaTech.Application.csproj GranjaTech.Application/
COPY GranjaTech.Api/GranjaTech.Api.csproj GranjaTech.Api/

# Restore
RUN dotnet restore GranjaTech.sln

# Copia todo o código
COPY . .

# Publish somente da API
RUN dotnet publish GranjaTech.Api/GranjaTech.Api.csproj -c Release -o /out

# Final
FROM base AS final
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet","GranjaTech.Api.dll"]
