# build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем ВЕСЬ репозиторий службы (UserService), чтобы были User.Core/Application/Infrastructure/Shared
COPY . .

# Переходим в папку entry-проекта и собираем ровно его
WORKDIR /src/UserService
RUN dotnet restore ./UserService.csproj
RUN dotnet publish ./UserService.csproj -c Release -o /app --no-restore

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8081
EXPOSE 8081
ENTRYPOINT ["dotnet", "UserService.dll"]
