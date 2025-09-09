# Base runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# copia só o csproj p/ cache de restore
COPY GranjaTech.Api.csproj ./
RUN dotnet restore GranjaTech.Api.csproj
# agora copia o resto
COPY . .
RUN dotnet publish GranjaTech.Api.csproj -c Release -o /out

# Final
FROM base AS final
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet","GranjaTech.Api.dll"]
