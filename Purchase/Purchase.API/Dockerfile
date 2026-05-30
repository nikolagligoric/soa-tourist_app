FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Purchase.API/Purchase.API.csproj Purchase.API/
COPY Purchase.Application/Purchase.Application.csproj Purchase.Application/
COPY Purchase.Domain/Purchase.Domain.csproj Purchase.Domain/
COPY Purchase.Infrastructure/Purchase.Infrastructure.csproj Purchase.Infrastructure/

RUN dotnet restore Purchase.API/Purchase.API.csproj

COPY . .

RUN dotnet build Purchase.API/Purchase.API.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish Purchase.API/Purchase.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Purchase.API.dll"]