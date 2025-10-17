FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 3006

USER root

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .

RUN apt-get update && apt-get install -y curl \
    && curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
    && apt-get install -y nodejs \
    && npm install -g npm
    
RUN dotnet restore "WaxMapArt.Web/WaxMapArt.Web.csproj"

WORKDIR "/src/WaxMapArt.Web"
RUN npm install
RUN dotnet publish "WaxMapArt.Web.csproj" \
    -c Release \
    -o /app/publish \
    -p:PublishTrimmed=false \
    -p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .
COPY --from=build /src/WaxMapArt.Web/node_modules /app/node_modules
ENTRYPOINT ["dotnet", "WaxMapArt.Web.dll"]
