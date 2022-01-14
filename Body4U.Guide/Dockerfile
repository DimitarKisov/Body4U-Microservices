FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Body4U.Guide/Body4U.Guide.csproj", "Body4U.Guide/"]
COPY ["Body4U.Common/Body4U.Common.csproj", "Body4U.Common/"]
RUN dotnet restore "Body4U.Guide/Body4U.Guide.csproj"
COPY . .
WORKDIR "/src/Body4U.Guide"
RUN dotnet build "Body4U.Guide.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Body4U.Guide.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Body4U.Guide.dll"]