FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["WaxMapArt.Bot/WaxMapArt.Bot.csproj", "WaxMapArt.Bot/"]
COPY ["WaxMapArt/WaxMapArt.csproj", "WaxMapArt/"]
RUN dotnet restore "WaxMapArt.Bot/WaxMapArt.Bot.csproj"
COPY . .
WORKDIR "/src/WaxMapArt.Bot"
RUN dotnet build "WaxMapArt.Bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WaxMapArt.Bot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WaxMapArt.Bot.dll"]
