# Конвенции проекта (для всех, кто промптит)

Читай этот файл в начале сессии. Он держит четырёх агентов в одном контексте,
чтобы код не расходился по стилю и не ломался контракт.

## Что за проект
PlantCare — помощник по уходу за растениями (хакатон). Подробности в [README.md](README.md).

## Структура
- `server/` — ASP.NET Core 10 Web API + EF Core (PostgreSQL), три проекта в одном решении:
  - `server/API/` — контроллеры, DI-конфигурация (`Program.cs`), Dtos, Fixtures для мока распознавания.
  - `server/Core/` — модели БД (`Models/`) и интерфейсы сервисов (`Services/Interfaces/`).
  - `server/DataAccess/` — `DbContext`, реализации сервисов (`Services/`), сид (`Configurations/SeedData.cs`), фоновые задачи (`Services/Background/`).
  
  Порт **5071**. БД поднимается через `docker-compose up -d` (PostgreSQL, порт 5432).
- `client/` — React + Vite + JavaScript + CSS + React Router. Порт **5173**.

## Железные правила
1. **Контракт API — единый источник правды.** Меняешь модель/DTO на бэке →
   сразу правишь `client/src/api/client.js`. Не расходитесь.
   (`client.js` — фасад, собранный из типизированных модулей `plantsApi.js`/`authApi.js`/
   `favoritesApi.js`/... поверх общего `apiClient.js`; страницы импортируют только `client.js`.)
2. **Не лезь в чужие файлы.** Держись своей зоны (см. таблицу в README).
   Общие точки (`server/API/Program.cs`, `client/src/App.jsx`, `server/DataAccess/DbContext.cs`)
   меняем аккуратно и предупреждаем команду.
3. **Мелкие частые коммиты, узкие ветки по фиче.** Четыре агента быстро создают конфликты.
4. **Владелец коллекции — id из JWT-куки `access_token`** (`OwnerIdExtensions.GetOwnerId`,
   заголовка `X-User-Id` больше нет). Гость работает без аккаунта: его коллекция/избранное —
   в localStorage (`client/src/api/guestStore.js`), фасад `client.js` переключает хранилище
   по auth-состоянию сам. Не хардкодь пользователя и не ходи в guestStore мимо фасада.

## Бэкенд-стиль
- Слоистая структура на 3 проекта: `Core` (модели + интерфейсы сервисов) ← `DataAccess`
  (реализации сервисов + `DbContext`) ← `API` (контроллеры, DI).
- Работа с базой данных происходит через **Services** (Сервисы), контроллеры инжектируют интерфейсы
  из `Core/Services/Interfaces`, а не `AppDbContext` напрямую.
- Ответы контроллеров: сущности/модели напрямую или Dtos (для записи).
- Enum'ы сериализуются строками (`"Watering"`). Циклы ссылок отключены (`IgnoreCycles`).
- База данных: PostgreSQL. Запуск через Docker (`docker-compose up -d`). Схема сейчас накатывается
  через `db.Database.EnsureCreated()` в `Program.cs` (не через EF-миграции) — если переходите на
  `dotnet ef migrations add`, не забудьте заменить `EnsureCreated()` на `Database.Migrate()`.

## Фронтенд-стиль
- Все запросы — через `client/src/api/client.js` (`api.plants`, `api.userPlants`, `api.recognition`, ...).
  Не пиши `fetch` напрямую в компонентах.
- Стили — обычный CSS (`client/src/App.css`, `client/src/index.css`). Tailwind не используем.
- Одна страница = один файл в `client/src/pages/` (`.jsx`). Роуты — в `App.jsx`.
- **Два режима хранения:** залогиненный пользователь — всё через бэкенд; гость — коллекция,
  избранное и даты ухода в `localStorage` (`guestStore.js`). Страницы разницы не видят —
  переключение внутри фасада `client.js` (`setGuestMode`). При входе гостевые данные
  импортируются в аккаунт (`api.guest.importToAccount`).

## Команды запуска и проверки
- Запуск БД (PostgreSQL): `docker-compose up -d`
- Запуск бэкенда в режиме разработки (с Hot Reload): `dotnet watch --project server/API/API.csproj`
- Запуск бэкенда обычный: `dotnet run --project server/API/API.csproj`
- Проверка сборки бэкенда: `dotnet build server/API/API.csproj`
- Запуск фронтенда: `cd client && npm run dev`
- Сборка фронтенда: `cd client && npm run build`

## Чего НЕ делаем
- Не пишем свой ML для распознавания фото — только внешний API.
- Не добавляем тяжёлые зависимости без согласования (следим за временем хакатона).
