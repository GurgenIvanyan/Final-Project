# build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем ВЕСЬ репозиторий службы (PlayListService), чтобы были Core/Application/Infrastructure
COPY . .

# Переходим в папку entry-проекта и собираем ровно его
WORKDIR /src/Playlist.Api
RUN dotnet restore ./Playlist.Api.csproj
RUN dotnet publish ./Playlist.Api.csproj -c Release -o /app --no-restore

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Playlist.Api.dll"]
