FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/DolarBot/", "DolarBot/"]
COPY ["src/DolarBot.Addons/DolarBot.Addons.csproj", "DolarBot.Addons/"]
COPY ["src/DolarBot.API/DolarBot.API.csproj", "DolarBot.API/"]
COPY ["src/DolarBot.Util/DolarBot.Util.csproj", "DolarBot.Util/"]
COPY ["src/DolarBot.Modules/DolarBot.Modules.csproj", "DolarBot.Modules/"]
COPY ["src/DolarBot.Services/DolarBot.Services.csproj", "DolarBot.Services/"]
RUN dotnet restore "DolarBot/DolarBot.csproj"
WORKDIR "/src/DolarBot"
COPY . .
RUN dotnet build "DolarBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DolarBot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DolarBot.dll"]