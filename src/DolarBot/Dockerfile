#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DolarBot/DolarBot.csproj", "DolarBot/"]
COPY ["DolarBot.Addons/DolarBot.Addons.csproj", "DolarBot.Addons/"]
COPY ["DolarBot.API/DolarBot.API.csproj", "DolarBot.API/"]
COPY ["DolarBot.Util/DolarBot.Util.csproj", "DolarBot.Util/"]
COPY ["DolarBot.Modules/DolarBot.Modules.csproj", "DolarBot.Modules/"]
COPY ["DolarBot.Services/DolarBot.Services.csproj", "DolarBot.Services/"]
RUN dotnet restore "DolarBot/DolarBot.csproj"
COPY . .
WORKDIR "/src/DolarBot"
RUN dotnet build "DolarBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DolarBot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DolarBot.dll"]