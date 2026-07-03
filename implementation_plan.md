# Архитектурная реорганизация бэкенда (PostgreSQL, Docker, Services, Entities)

Этот план описывает переработку бэкенда для перехода со SQLite на PostgreSQL, контейнеризацию проекта с использованием Docker, выделение четких слоев бизнес-логики (Services) и сущностей БД (Entities), а также проектирование базы данных для поддержки усложнений (обмен, чат, рекомендации, авторизация).

## Требуется согласование с пользователем

> [!IMPORTANT]
> **Реорганизация структуры проекта:**
> В данный момент весь бэкенд находится в одном проекте [PlantCare.Api.csproj](file:///d:/Hackathon-132/server/PlantCare.Api.csproj). Мы рекомендуем **сохранить единый проект**, но логически разделить его на папки:
> * `Entities/` — модели базы данных (вместо `Models/`).
> * `Services/` — бизнес-логика (интерфейсы и реализации).
> * `Data/` — контекст БД, миграции и сидер.
> * `Controllers/` — API-контроллеры (вызывают сервисы).
> * `Dtos/` — объекты переноса данных для запросов и ответов.
>
> Это сэкономит время на хакатоне по сравнению с разделением на 3-4 отдельных проекта в решении (.sln).

> [!WARNING]
> **Подход к чату обмена:**
> Чат для обмена растениями можно реализовать двумя способами:
> 1. **REST API (Опросы / Поллинг):** Более простой в реализации, сообщения запрашиваются фронтендом по интервалу.
> 2. **SignalR (WebSockets):** Real-time чат. Требует дополнительной настройки CORS и хаба на бэкенде.
> *Рекомендуется начать с REST API как базового варианта, а при наличии времени перейти на SignalR.*

## Открытые вопросы

> [!NOTE]
> **Алгоритм рекомендаций:**
> Какую логику рекомендаций мы закладываем? Например, предлагать растения из справочника, у которых требования к освещению (`Light`) или частоте полива (`WateringFrequencyDays`) совпадают с имеющейся коллекцией пользователя, но которых у него ещё нет.

---

## Предлагаемые изменения

### 1. Переход на PostgreSQL и EF Core Npgsql
Для интеграции PostgreSQL необходимо заменить провайдер БД со SQLite на PostgreSQL.

#### [MODIFY] [PlantCare.Api.csproj](file:///d:/Hackathon-132/server/PlantCare.Api.csproj)
* Удалить `Microsoft.EntityFrameworkCore.Sqlite`.
* Добавить пакет `Npgsql.EntityFrameworkCore.PostgreSQL` (версии `10.0.0` или `9.0.x` в зависимости от версии SDK).

#### [MODIFY] [Program.cs](file:///d:/Hackathon-132/server/Program.cs)
* Заменить `builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(...))` на `o.UseNpgsql(...)`.
* Обновить механизм миграций: заменить `db.Database.EnsureCreated()` на `db.Database.Migrate()` для поддержки полноценных миграций PostgreSQL.

#### [MODIFY] [appsettings.json](file:///d:/Hackathon-132/server/appsettings.json)
* Изменить строку подключения `Default` на PostgreSQL-совместимую:
  `"Host=db;Port=5432;Database=plantcare;Username=postgres;Password=postgres"` (параметры для Docker).

---

### 2. Docker и Docker Compose
Контейнеризация приложения для легкого развертывания бэкенда и БД.

#### [NEW] [Dockerfile](file:///d:/Hackathon-132/server/Dockerfile)
* Многоэтапный (multi-stage) Dockerfile для сборки ASP.NET Core 10 приложения.
* Экспонирует порт `5071` (или `8080` внутри контейнера с пробросом на `5071`).

#### [NEW] [docker-compose.yml](file:///d:/Hackathon-132/docker-compose.yml)
* Описание двух сервисов:
  1. `db`: образ `postgres:15-alpine` с настроенными переменными окружения.
  2. `web`: наш API бэкенд, зависящий от `db` (с `depends_on` и проверкой готовности `healthcheck`).

---

### 3. Структура файлов и Выделение Слоя Сервисов (Services)
Мы перерабатываем структуру папок в [server](file:///d:/Hackathon-132/server) следующим образом:

```
server/
├── Controllers/         # API эндпоинты (вызывают исключительно интерфейсы сервисов)
├── Data/                # DbContext, SeedData, Migrations/
├── Dtos/                # Запросы/ответы (AuthDto, ExchangeDto и др.)
├── Entities/            # Сущности БД (переименовано из Models/)
│   ├── User.cs
│   ├── Plant.cs
│   ├── UserPlant.cs
│   ├── Reminder.cs
│   ├── Favorite.cs
│   ├── ExchangeOffer.cs # NEW: Предложение обмена
│   └── ChatMessage.cs   # NEW: Сообщение в чате обмена
└── Services/            # NEW: Бизнес-логика (Service Layer)
    ├── Interfaces/      # IAuthService, IPlantService, IExchangeService, ...
    ├── Implementations/ # Реализация сервисов
    └── Background/      # BackgroundService для проверки напоминаний
```

#### Создание новых сущностей (Entities):
1. **[ExchangeOffer.cs](file:///d:/Hackathon-132/server/Entities/ExchangeOffer.cs)**:
   * `Id`, `OwnerId` (кто меняет), `UserPlantId` (какое растение отдаёт), `WantedPlantDescription` (что хочет взамен), `Status` (Active, Completed, Cancelled), `CreatedAt`.
2. **[ChatMessage.cs](file:///d:/Hackathon-132/server/Entities/ChatMessage.cs)**:
   * `Id`, `ExchangeOfferId`, `SenderId`, `ReceiverId`, `Text`, `SentAt`, `IsRead`.

#### Создание Сервисов (Services):
1. **`AuthService`**: Регистрация, логин (хеширование паролей через `BCrypt.Net-Next` или `Identity`, генерация JWT токенов).
2. **`UserPlantService`**: Логика добавления растений в коллекцию, автоматический расчет даты следующего полива/пересадки на основе данных справочника.
3. **`ReminderService` & `ReminderScheduler`**:
   * API для отметки напоминаний выполненными.
   * Фоновый сервис (`IHostedService` / `BackgroundService`), который может раз в сутки проверять просроченные напоминания и, например, логировать уведомления (или готовить их для отправки на фронтенд).
4. **`ExchangeService`**: Управление предложениями обмена и отправкой/получением сообщений чата.
5. **`RecommendationService`**: Подбор рекомендаций на основе имеющихся в коллекции параметров ухода.

---

### 4. Обновление [CLAUDE.md](file:///d:/Hackathon-132/CLAUDE.md)
Необходимо зафиксировать новые правила разработки:
* Использование сервисов вместо прямого вызова `DbContext` в контроллерах.
* Использование DTO для передачи данных.
* Настройки запуска через `docker-compose`.
* Хеширование паролей и авторизация по JWT.

---

## План Верификации

### Автоматические тесты
* Выполнение сборки бэкенда:
  ```bash
  cd server
  dotnet build
  ```
* Запуск контейнеров для проверки сборки Docker образа:
  ```bash
  docker-compose build
  ```

### Ручная проверка
1. Проверка запуска Docker Compose и успешного прохождения миграций в PostgreSQL.
2. Проверка автоматического наполнения справочника (`SeedData`) при первом запуске в PostgreSQL.
3. Проверка работоспособности Scalar UI (`http://localhost:5071/scalar/v1`) для новых эндпоинтов обмена и чата.
