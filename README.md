# 🌿 PlantCare — помощник по уходу за растениями

Хакатон-проект. Справочник растений + личная коллекция + напоминания об уходе.

## Стек

| Слой | Технологии |
|------|-----------|
| Frontend | React 19 + Vite + TypeScript + Tailwind v4 + React Router |
| Backend | ASP.NET Core 10 Web API + EF Core |
| БД | SQLite (старт) → PostgreSQL (переключается заменой провайдера) |
| Контракт | OpenAPI (`/openapi/v1.json`) + Scalar UI (`/scalar/v1`) |

Монорепо: `client/` (фронт) + `server/` (бэк).

## Запуск

Нужны: **.NET 10 SDK**, **Node 20+**.

**Бэкенд** (порт 5071):
```bash
cd server
dotnet run
```
- Swagger/Scalar UI: http://localhost:5071/scalar/v1
- OpenAPI JSON: http://localhost:5071/openapi/v1.json
- БД (`plantcare.db`) создаётся и наполняется справочником автоматически при первом старте.

**Фронтенд** (порт 5173):
```bash
cd client
npm install   # если ещё не ставили
npm run dev
```
Открыть http://localhost:5173. Базовый URL API берётся из `client/.env` (`VITE_API_URL`).

## Архитектура и контракт

Единый источник правды по API — **OpenAPI-схема бэка**. Фронт зеркалит её в
[`client/src/api/types.ts`](client/src/api/types.ts). При изменении бэка обновляйте типы
(вручную или сгенерируйте из `/openapi/v1.json`, напр. `npx openapi-typescript`).

Идентификация пользователя в базовой версии — заголовок `X-User-Id`
(по умолчанию `"local"`, без аккаунтов). Усложнение «авторизация» подставит реальный id.

### Основные эндпоинты

| Метод | Путь | Назначение |
|-------|------|-----------|
| GET | `/api/plants?search=&difficulty=` | справочник (поиск/фильтр) |
| GET | `/api/plants/{id}` | карточка растения |
| GET/POST/PUT/DELETE | `/api/userplants` | личная коллекция |
| GET/POST/PUT/DELETE | `/api/reminders` | напоминания |
| POST | `/api/reminders/{id}/done` | отметить выполненным (сдвиг срока) |
| GET/POST/DELETE | `/api/favorites` | избранное |

## Распределение по людям (4)

| Кто | Зона | Основные файлы |
|-----|------|----------------|
| **1. Backend + БД** | схема, эндпоинты, сид, деплой | `server/**` |
| **2. Frontend — справочник** | app shell, каталог, карточка, избранное | `client/src/App.tsx`, `pages/CatalogPage.tsx`, `pages/FavoritesPage.tsx` |
| **3. Frontend — коллекция** | «мои растения», напоминания, уведомления | `pages/MyPlantsPage.tsx`, `pages/RemindersPage.tsx` |
| **4. Full-stack — усложнения** | авторизация → фото → рекомендации → обмен+чат | новые модули, флаги фич |

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
латыни и по `GbifId`. Порог уверенности — в конфиге (`PlantNet:ConfidenceThreshold`).

### Ключ и режимы
- **Без ключа работает сразу** на фикстурах (`server/Fixtures/*.json`) — авто-мок.
- Ключ задавать **не в git**, а через user-secrets или env:
  ```bash
  cd server
  dotnet user-secrets init
  dotnet user-secrets set "PlantNet:ApiKey" "ВАШ_КЛЮЧ"
  ```
  (или переменная окружения `PlantNet__ApiKey`). Как только ключ есть — запросы идут в реальный API.
- Форсировать мок на демо (без сети): `PlantNet:UseMock = true`.

### Как проверить
- **Реальный API изолированно** (после получения ключа):
  `cd server && bash test-plantnet.sh ВАШ_КЛЮЧ` — дёрнет Pl@ntNet напрямую примером фото.
- **Наш эндпоинт в мок-режиме** (без ключа): `?scenario=` выбирает фикстуру —
  `monstera` (Matched), `secondmatch` (Matched через обход), `unknown` (нет карточки), `lowconfidence`:
  ```bash
  curl -X POST "http://localhost:5071/api/recognition/identify?scenario=monstera" \
    -F "image=@photo.jpg" -F "organ=auto"
  ```
- **Реальный флоу**: с заданным ключом отправить настоящее фото без `scenario`.

## Дорожная карта усложнений (потолок коэф. 1.5)

1. ✅ Сервер + синхронизация (уже заложено этой архитектурой)
2. ☐ Авторизация (`User`-модель уже есть, нужен endpoint + JWT)
3. ✅ Распознавание по фото — Pl@ntNet (см. раздел выше), без своего ML
4. ☐ Рекомендации — простые правила по свету/поливу
5. ☐ Обмен растениями + чат (дороже, realtime)

Берём по возрастанию цены балла, каждая фича — за флагом, чтобы не ломать базу.
