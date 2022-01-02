FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Body4U.EmailSender/Body4U.EmailSender.csproj", "Body4U.EmailSender/"]
COPY ["Body4U.Common/Body4U.Common.csproj", "Body4U.Common/"]
RUN dotnet restore "Body4U.EmailSender/Body4U.EmailSender.csproj"
COPY . .
WORKDIR "/src/Body4U.EmailSender"
RUN dotnet build "Body4U.EmailSender.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Body4U.EmailSender.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Body4U.EmailSender.dll"]