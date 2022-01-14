FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Body4U.Watchdog/Body4U.Watchdog.csproj", "Body4U.Watchdog/"]
RUN dotnet restore "Body4U.Watchdog/Body4U.Watchdog.csproj"
COPY . .
WORKDIR "/src/Body4U.Watchdog"
RUN dotnet build "Body4U.Watchdog.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Body4U.Watchdog.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Body4U.Watchdog.dll"]