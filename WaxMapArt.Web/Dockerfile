FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WaxMapArt.Web/WaxMapArt.Web.csproj", "WaxMapArt.Web/"]
COPY ["WaxMapArt/WaxMapArt.csproj", "WaxMapArt/"]
RUN dotnet restore "WaxMapArt.Web/WaxMapArt.Web.csproj"
COPY . .
WORKDIR "/src/WaxMapArt.Web"
RUN dotnet build "WaxMapArt.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WaxMapArt.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM buildbase as migrations
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
ENTRYPOINT dotnet-ef database update

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WaxMapArt.Web.dll"]