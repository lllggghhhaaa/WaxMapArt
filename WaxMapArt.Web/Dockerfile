FROM mcr.microsoft.com/dotnet/nightly/aspnet:10.0-preview AS base
USER $APP_UID
WORKDIR /app
EXPOSE 3006

FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0-preview AS build
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

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WaxMapArt.Web.dll"]
