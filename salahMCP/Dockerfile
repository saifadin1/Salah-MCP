FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["salahMCP.csproj", "./"]
RUN dotnet restore "./salahMCP.csproj"

COPY . .
RUN dotnet publish "./salahMCP.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV PRAYER_CITY=Cairo
ENV PRAYER_COUNTRY=Egypt

ENTRYPOINT ["dotnet", "salahMCP.dll"]