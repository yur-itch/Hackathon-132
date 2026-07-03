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

Единый источник правды по API — **бэкенд**. Все страницы обращаются к нему через фасад
[`client/src/api/client.js`](client/src/api/client.js) (`api.plants`, `api.userPlants`,
`api.reminders`, `api.favorites`, `api.exchange`, `api.push`, `api.recognition`, `api.auth`).
Сам фасад собран из типизированных модулей (`plantsApi.js`, `userPlantsApi.js`, `authApi.js`,
`exchangeApi.js`, `pushApi.js`, ...), каждый из которых ходит через общий низкоуровневый
`apiClient.js` (`apiRequest`) — там же разбор `ValidationProblemDetails` в читаемое сообщение.
Компоненты не дёргают `fetch`/`apiRequest`/типизированные модули напрямую — только `client.js`.

**Два режима работы.** Без аккаунта (базовый сценарий из ТЗ) доступны справочник,
«Мои растения», напоминания, избранное и распознавание — данные гостя живут в localStorage
браузера (`client/src/api/guestStore.js`), фасад `client.js` переключает хранилище по
auth-состоянию (`setGuestMode`), страницы разницы не видят. Рекомендации гостю считает
бэкенд по `?plantIds=` из его локальной коллекции. После входа/регистрации гостевые данные
автоматически импортируются в аккаунт (`api.guest.importToAccount`).

**За логином** остаются обмен растениями (нужна личность для объявлений и чата),
push-уведомления и синхронизация между устройствами. На бэке владелец коллекции берётся
строго из JWT-claim (`OwnerIdExtensions.GetOwnerId`) — заголовка `X-User-Id` больше нет,
пользовательский эндпоинт без валидной куки `access_token` вернёт 401. JWT-кука ставится
`AuthController` при логине/регистрации. Анонимно доступны только `/api/plants`,
`/api/recognition/identify` и `/api/recommendations` (с `plantIds`).

### Актуальные эндпоинты

| Метод | Путь | Назначение |
|-------|------|-----------|
| POST | `/api/auth/register`, `/login`, `/logout` | регистрация/логин (ставит куку `access_token`), логаут |
| GET/PUT | `/api/user/me` | профиль текущего пользователя |
| GET | `/api/plants` | справочник |
| GET | `/api/plants/{id}` | карточка растения |
| GET/POST/PUT/DELETE | `/api/user-plants` | личная коллекция (напоминания — поля `nextWateringDate`/`nextRepottingDate`, id связанных `Reminder` — на самой записи) |
| GET/POST/PUT/DELETE | `/api/reminders` | напоминания напрямую (фронт сейчас работает через `user-plants`, этим эндпоинтом не пользуется) |
| POST | `/api/reminders/{id}/done` | отметить выполненным (сдвигает срок) |
| GET/POST/DELETE | `/api/favorites` | избранное залогиненного (гость — в localStorage) |
| GET | `/api/recommendations?count=&plantIds=` | рекомендации: по коллекции из БД (JWT) или по `plantIds` (гость) |
| GET/POST/DELETE | `/api/exchange/offers`, `/messages`, `/chats`; POST `/confirm` | обмен растениями + чат по сделке |
| GET/POST | `/api/push/vapid-public-key`, `/subscribe`, `/unsubscribe` | Web Push подписки |
| POST | `/api/recognition/identify` | распознавание по фото (см. ниже) |

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

## Push-уведомления о напоминаниях

`ReminderBackgroundService` (`IHostedService`) раз в 12 часов проверяет просроченные
напоминания и шлёт Web Push через `IPushService` (библиотека `WebPush`, VAPID-ключи).
Подписка — кнопка на странице «Напоминания» (`enablePushNotifications()` →
`sw.js` → `PushManager.subscribe()` → `POST /api/push/subscribe`), хранится в таблице
`PushSubscriptions`, уникальность по `(OwnerId, Endpoint)` — на общем браузере разные
пользователи получают независимые подписки.

**Уведомление шлётся не более одного раза на конкретный срок** (`Reminder.NotifiedAt`
не даёт слать повторно каждые 12 часов). Если проигнорировать push — само напоминание
остаётся просроченным в приложении бессрочно, но больше не толкается; чтобы снова получить
уведомление, нужно нажать «Готово» (сдвигает срок вперёд и сбрасывает дедуп). Периодический
«дожим» (напоминать повторно, пока не отметят выполненным) пока не сделан — открытый
продуктовый вопрос, при желании легко добавить.

Секреты (`Jwt:Secret`, `Vapid:PublicKey`/`PrivateKey`) — только через user-secrets, см. выше.

## Дорожная карта усложнений (потолок коэф. 1.5)

1. ✅ Сервер + синхронизация (Postgres + Docker; коллекция, справочник и избранное — на бэке)
2. ✅ Авторизация — `AuthController`, JWT в cookie `access_token`; базовые вкладки работают
   и без аккаунта (гостевой режим на localStorage, см. «Два режима работы»)
3. ✅ Распознавание по фото — Pl@ntNet (см. раздел выше), без своего ML
4. ✅ Push-уведомления о напоминаниях — Web Push/VAPID (см. раздел выше)
5. ✅ Рекомендации — `RecommendationsController` + секция «Вам может подойти» в справочнике,
   работает и для гостя (по `plantIds` из localStorage-коллекции)
6. ✅ Обмен растениями + чат — `ExchangeController` + `ExchangePage.jsx` (роут `/exchange`,
   только для залогиненных)

Легенда: ✅ доступно через API и фронт · 🔶 почти готово, есть конкретный незакрытый хвост · ☐ не начато.
