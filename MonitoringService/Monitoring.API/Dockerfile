FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Monitoring.API/Monitoring.API.csproj", "Monitoring.API/"]

RUN dotnet restore "Monitoring.API/Monitoring.API.csproj"

COPY . .

WORKDIR "/src/Monitoring.API"

RUN dotnet build \
    "Monitoring.API.csproj" \
    -c Release \
    -o /app/build

FROM build AS publish

RUN dotnet publish \
    "Monitoring.API.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Monitoring.API.dll"]