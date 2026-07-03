# Сборка приложения
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Копируем проекты и восстанавливаем зависимости
COPY ["API/API.csproj", "API/"]
COPY ["Core/Core.csproj", "Core/"]
COPY ["DataAccess/DataAccess.csproj", "DataAccess/"]
RUN dotnet restore "API/API.csproj"

# Копируем исходный код остальных проектов
COPY . .
WORKDIR "/src/API"
RUN dotnet build "API.csproj" -c Release -o /app/build

# Публикация приложения
FROM build AS publish
RUN dotnet publish "API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Финальный образ для запуска
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Настройка переменных окружения для прослушивания порта 5071 внутри контейнера
ENV ASPNETCORE_URLS=http://+:5071
EXPOSE 5071

ENTRYPOINT ["dotnet", "API.dll"]
