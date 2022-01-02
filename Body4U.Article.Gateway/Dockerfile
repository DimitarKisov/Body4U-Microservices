FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Body4U.Article.Gateway/Body4U.Article.Gateway.csproj", "Body4U.Article.Gateway/"]
COPY ["Body4U.Common/Body4U.Common.csproj", "Body4U.Common/"]
RUN dotnet restore "Body4U.Article.Gateway/Body4U.Article.Gateway.csproj"
COPY . .
WORKDIR "/src/Body4U.Article.Gateway"
RUN dotnet build "Body4U.Article.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Body4U.Article.Gateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Body4U.Article.Gateway.dll"]