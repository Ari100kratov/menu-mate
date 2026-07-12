# Развёртывание

## Локальная разработка

Для локального запуска используется Aspire:

```powershell
dotnet run --project src/MenuMate.AppHost/MenuMate.AppHost.csproj
```

AppHost поднимает PostgreSQL, MinIO, одноразовую инициализацию бакетов, мигратор, API и Vite-фронтенд.

## Docker Compose и Portainer

Compose-стек содержит PostgreSQL, MinIO, `minio-init`, `migrator`, API и production-фронтенд. Последний собирает Vite-приложение и отдаёт его через Nginx; запросы `/api/*` и `/openapi/*` Nginx проксирует во внутренний контейнер API.

```powershell
copy .env.example .env
docker compose up -d --build
```

После запуска приложение доступно на `http://<host>:<WEB_PORT>`. Порт API публикуется отдельно только для диагностики и интеграций: браузерный фронтенд использует относительный путь `/api`.

Для production перед Nginx-контейнером нужен reverse proxy с TLS. Это необходимо и для безопасных cookie авторизации. Не публикуйте PostgreSQL и MinIO наружу без необходимости.

## Публичный адрес MinIO

Контейнер API использует внутренний адрес `minio:9000` для загрузки объектов. Но подписанные ссылки открывает браузер, которому этот DNS-адрес недоступен. Поэтому в `.env` задаётся отдельный адрес:

```dotenv
MINIO_PUBLIC_ENDPOINT=storage.example.com
MINIO_PUBLIC_USE_SSL=true
```

Значение указывается без `https://`. Домен должен вести на MinIO API и быть доступен из браузера. Добавьте публичный адрес фронтенда в `MINIO_API_CORS_ALLOW_ORIGIN`, если изображения запрашиваются напрямую с другого origin.

## Миграции и хранилище

Миграции выполняет отдельный одноразовый контейнер `migrator`; API стартует только после его успешного завершения. `minio-init` ожидает health check MinIO и затем создаёт бакеты `images` и `imports`.

Планируйте резервное копирование Docker volumes PostgreSQL и MinIO, а секреты задавайте через переменные окружения или менеджер секретов.
