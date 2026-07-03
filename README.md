# 🌿 PlantCare — помощник по уходу за растениями

Хакатон-проект. Справочник растений + личная коллекция + напоминания об уходе.

## Стек

| Слой | Технологии |
|------|-----------|
| Frontend | React 19 + Vite + JavaScript + CSS + React Router |
| Backend | ASP.NET Core 10 Web API + EF Core, 3 проекта (API / Core / DataAccess) |
| БД | PostgreSQL (через Docker Compose) |
| Контракт | OpenAPI (`/openapi/v1.json`) + Scalar UI (`/scalar/v1`) |

Монорепо: `client/` (фронт) + `server/` (бэк: `API/`, `Core/`, `DataAccess/`).

## Запуск

Нужны: **.NET 10 SDK**, **Node 20+**, **Docker** (для БД).

**База данных:**
```bash
docker-compose up -d
```
Поднимает PostgreSQL на `localhost:5432` (см. `docker-compose.yml`).

**Бэкенд** (порт 5071):
```bash
dotnet run --project server/API/API.csproj
# или с hot reload:
dotnet watch --project server/API/API.csproj
```
- Swagger/Scalar UI: http://localhost:5071/scalar/v1
- OpenAPI JSON: http://localhost:5071/openapi/v1.json
- Схема БД и справочник (`SeedData`) создаются автоматически при первом старте
  (`Database.EnsureCreated()` — полноценных EF-миграций пока нет). **Важно:** если после
  `git pull` видите ошибки `column/relation does not exist` — значит в моделях появились
  новые таблицы/колонки, а `EnsureCreated()` их в уже существующую БД не добавляет.
  Лечится сносом локального volume: `docker compose down -v && docker compose up -d db`.
- Секреты (`Jwt:Secret`, `Vapid:PublicKey`/`PrivateKey`, `PlantNet:ApiKey`) — **не в git**,
  через user-secrets:
  ```bash
  cd server/API
  dotnet user-secrets init
  dotnet user-secrets set "Jwt:Secret" "любая случайная строка"
  dotnet user-secrets set "Vapid:PublicKey" "..."
  dotnet user-secrets set "Vapid:PrivateKey" "..."
  ```
  Без `Jwt:Secret` авторизация не заработает; без VAPID-ключей push тихо не будет отправляться
  (см. лог). Ключи одной пары VAPID должны быть одинаковыми у всех, кто тестирует push на общем
  фронте — сгенерировать новую пару можно через `WebPush.VapidHelper.GenerateVapidKeys()`.

**Фронтенд** (порт 5173):
```bash
cd client
npm install   # если ещё не ставили
npm run dev
```
Открыть http://localhost:5173. Базовый URL API берётся из `client/.env` (`VITE_API_URL`).

## Архитектура и контракт

Единый источник правды по API — **бэкенд**. Фронт обращается к нему через
[`client/src/api/client.js`](client/src/api/client.js) (единственный реально используемый
API-клиент — файлы `apiClient.js`/`plantsApi.js` в той же папке не импортируются нигде и
являются черновиком, оставленным по ходу разработки).

Идентификация пользователя в базовой версии — заголовок `X-User-Id`
(по умолчанию `"local"`, без аккаунтов). Усложнение «авторизация» подставит реальный id
(сервис и JWT-логика уже есть — `AuthService`, — но HTTP-эндпоинта под него пока нет).

### Актуальные эндпоинты

| Метод | Путь | Назначение |
|-------|------|-----------|
| GET | `/api/plants?search=&isPoisonous=` | справочник (поиск/фильтр) |
| GET | `/api/plants/{id}` | карточка растения |
| GET/POST/PUT/DELETE | `/api/user-plants` | личная коллекция (напоминания — поля `nextWateringDate`/`nextRepottingDate` на самой записи) |
| POST | `/api/recognition/identify` | распознавание по фото (см. ниже) |

**Не подключено к HTTP (сервис/логика есть, контроллера нет):** авторизация (`AuthService`),
рекомендации (`RecommendationService`), избранное (`FavoritesService`), обмен растениями + чат
(`ExchangeService`, `ExchangeOffer`/`ChatMessage`). Фоновая проверка напоминаний
(`ReminderBackgroundService`) зарегистрирована как `IHostedService` и уже крутится в фоне.

**Избранное** на фронте пока работает через `localStorage` браузера
(`api.favorites` в `client.js`), без обращения к серверу — хотя `FavoritesService` на бэке
уже есть, к нему просто ещё не подключились.

## Распределение по людям (4)

| Кто | Зона | Основные файлы |
|-----|------|----------------|
| **1. Backend — API-слой** | контроллеры, эндпоинты, DI | `server/API/**` |
| **2. Backend — остальное** | сервисы, БД, инфраструктура, деплой | `server/Core/**`, `server/DataAccess/**`, `docker-compose.yml` |
| **3. Full-stack — усложнения** | авторизация → фото → рекомендации → обмен+чат | новые модули, флаги фич |
| **4. Frontend** | весь клиент: справочник, коллекция, напоминания, избранное | `client/**` |

## Распознавание растений по фото (Pl@ntNet)

Усложнение: пользователь загружает фото → бэк проксирует в [Pl@ntNet](https://my.plantnet.org)
(500 распознаваний/день бесплатно) → матчит вид со справочником → отдаёт статус.

**Эндпоинт:** `POST /api/recognition/identify` (multipart: `image` файл, `organ` опц.)

Возвращает `RecognitionResult` с одним из статусов (фронт рисует свой экран под каждый):

| Status | Что значит | UX |
|--------|-----------|-----|
| `Matched` | вид определён, карточка есть | карточка + «Добавить в список» |
| `RecognizedButNoCard` | вид определён, карточки нет (частый штатный исход) | «Определили как X, карточки нет» + ручной поиск |
| `LowConfidence` | не растение / размыто (топ score < 0.3) | «Попробуйте другое фото» |
| `Failed` | реальный сбой API (таймаут/лимит/ключ) | мягкая ошибка, не 500 |

Матчинг устойчив: обходит всех кандидатов (не только топ), матчит по нормализованной
латыни и по `GbifId`. Уверенность (`topScore`) считается по реально сматченной карточке,
а не по топ-кандидату — если карточка нашлась у второго/третьего кандидата, в ответе будет
именно его score. Порог уверенности — в конфиге (`PlantNet:ConfidenceThreshold`).

### Ключ и режимы
- **Без ключа работает сразу** на фикстурах (`server/API/Fixtures/*.json`) — авто-мок.
- Ключ задавать **не в git**, а через user-secrets или env:
  ```bash
  cd server/API
  dotnet user-secrets init
  dotnet user-secrets set "PlantNet:ApiKey" "ВАШ_КЛЮЧ"
  ```
  (или переменная окружения `PlantNet__ApiKey`). Как только ключ есть — запросы идут в реальный API,
  а `?scenario=` (мок) перестаёт учитываться.
- Форсировать мок при наличии ключа (демо без сети): `PlantNet:UseMock = true` в `server/API/appsettings.json`.

### Как проверить
- **Реальный API изолированно** (после получения ключа):
  `cd server && bash test-plantnet.sh ВАШ_КЛЮЧ` — дёрнет Pl@ntNet напрямую примером фото.
- **Наш эндпоинт в мок-режиме** (без ключа или с `PlantNet:UseMock = true`): `?scenario=` выбирает фикстуру —
  `monstera` (Matched), `secondmatch` (Matched через обход кандидатов), `unknown` (нет карточки), `lowconfidence`:
  ```bash
  curl -X POST "http://localhost:5071/api/recognition/identify?scenario=monstera" \
    -F "image=@photo.jpg" -F "organ=auto"
  ```
- **Реальный флоу**: с заданным ключом отправить настоящее фото без `scenario`.

## Дорожная карта усложнений (потолок коэф. 1.5)

1. ✅ Сервер + синхронизация (Postgres + Docker, коллекция и справочник синхронизированы;
   избранное — исключение, оно пока только в localStorage)
2. 🔶 Авторизация — `AuthService` (регистрация/логин, BCrypt, JWT) реализован, но нет
   `AuthController` и `UseAuthentication/UseAuthorization` в `Program.cs`
3. ✅ Распознавание по фото — Pl@ntNet (см. раздел выше), без своего ML
4. 🔶 Рекомендации — `RecommendationService` реализован (правила по сложности ухода и
   частоте полива относительно коллекции пользователя), но нет `RecommendationController`
5. 🔶 Обмен растениями + чат — модели `ExchangeOffer`/`ChatMessage` и `ExchangeService`
   реализованы, контроллера и фронта пока нет

Легенда: ✅ доступно через API и фронт · 🔶 бизнес-логика есть, но не подключена к HTTP · ☐ не начато.
