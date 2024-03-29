﻿FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["recal-social-api/recal-social-api.csproj", "./"]
RUN dotnet restore "recal-social-api.csproj"
COPY recal-social-api/. .
WORKDIR "/src/"
RUN dotnet build "recal-social-api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "recal-social-api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "recal-social-api.dll"]
