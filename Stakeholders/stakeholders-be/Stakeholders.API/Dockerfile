FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore Stakeholders.API/Stakeholders.API.csproj
RUN dotnet build Stakeholders.API/Stakeholders.API.csproj -c Release

FROM build AS publish
RUN dotnet publish Stakeholders.API/Stakeholders.API.csproj -c Release -o /app/publish

ENV ASPNETCORE_URLS=http://+:80

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

CMD ["dotnet", "Stakeholders.API.dll"]