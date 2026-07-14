# Развёртывание

## Локальная разработка

Для локального запуска используется Aspire:

```powershell
dotnet run --project src/MenuMate.AppHost/MenuMate.AppHost.csproj
```

AppHost поднимает PostgreSQL, MinIO, одноразовую инициализацию бакетов, мигратор, API и Vite-фронтенд.

## Docker Compose и Portainer

Compose-стек рассчитан на Portainer с окружением Docker Standalone и содержит PostgreSQL, MinIO, `minio-init`, `migrator`, API, production-фронтенд и standalone Aspire Dashboard. Фронтенд собирает Vite-приложение и отдаёт его через Nginx; запросы `/api/*` Nginx проксирует во внутренний контейнер API, а `/images/*` и `/imports/*` — во внутренний MinIO.

```powershell
copy .env.example .env
docker compose up -d --build
```

После запуска приложение доступно на `http://<host>:<WEB_PORT>`. Браузерный фронтенд использует относительный путь `/api`. Диагностические порты API, PostgreSQL и MinIO привязаны к `127.0.0.1` Linux-хоста и не должны публиковаться во внешнюю сеть.

Для production перед Nginx-контейнером нужен reverse proxy с TLS. Это необходимо и для безопасных cookie авторизации. Внешний proxy обязан сохранять `Host`, `X-Forwarded-For` и `X-Forwarded-Proto`. Внутренний Nginx сохраняет исходную HTTPS-схему при передаче запроса в API.

Если reverse proxy работает непосредственно на Linux-хосте, задайте `WEB_BIND_ADDRESS=127.0.0.1`. Если proxy работает в другом контейнере и обращается к опубликованному порту хоста, используется `WEB_BIND_ADDRESS=0.0.0.0`; доступ к порту нужно ограничить firewall-ом.

## Aspire Dashboard в production

Dashboard запускается отдельным контейнером и получает OTLP-телеметрию от API и мигратора по внутреннему адресу `http://aspire-dashboard:18889`. OTLP-порт не публикуется на хосте и дополнительно защищён отдельным API key. UI защищён browser token и должен открываться только через отдельный HTTPS-домен.

Перед развёртыванием добавьте в Portainer:

```dotenv
ASPIRE_DASHBOARD_PUBLIC_URL=https://aspire.example.com
ASPIRE_DASHBOARD_BIND_ADDRESS=0.0.0.0
ASPIRE_DASHBOARD_PORT=18888
ASPIRE_DASHBOARD_BROWSER_TOKEN=<64 случайных hex-символа>
ASPIRE_DASHBOARD_OTLP_API_KEY=<другие 64 случайных hex-символа>
```

Для генерации секретов на Linux можно использовать:

```bash
openssl rand -hex 32
```

Reverse proxy направляет домен Dashboard на `<linux-host>:<ASPIRE_DASHBOARD_PORT>`, передаёт `Host`, `X-Forwarded-For`, `X-Forwarded-Proto` и поддерживает WebSocket. OTLP-порт `18889` в proxy и firewall открывать не требуется.

Не задавайте `ASPIRE_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true` в production. Standalone Dashboard показывает структурированные логи, трейсы и метрики, но не предоставляет локальный список ресурсов AppHost. Телеметрия хранится только в памяти и очищается при рестарте контейнера; Dashboard используется для оперативной диагностики, а не как долговременное хранилище.

OpenAPI и Scalar в production отключены. Для временной диагностики их можно включить через `EXPOSE_API_DOCS=true`, после чего снова отключить.

## Проксирование изображений MinIO

Контейнер API использует внутренний адрес `minio:9000` для загрузки объектов. Но подписанные ссылки открывает браузер, которому этот DNS-адрес недоступен. Nginx фронтенда проксирует пути бакетов `/images/*` и `/imports/*` во внутренний MinIO, поэтому ссылки используют домен самого приложения:

```dotenv
MINIO_PUBLIC_ENDPOINT=menu.example.com
MINIO_PUBLIC_USE_SSL=true
```

Значение указывается без `https://`. Не указывайте `minio:9000` или отдельный домен хранилища: домен должен совпадать с публичным доменом фронтенда. Nginx сохраняет заголовок `Host`, поскольку он участвует в подписи ссылки MinIO. При внешнем reverse proxy также передавайте исходный `Host` в контейнер фронтенда.

## Миграции и хранилище

Миграции выполняет отдельный одноразовый контейнер `migrator`; API стартует только после его успешного завершения. `minio-init` ожидает health check MinIO и затем создаёт бакеты `images` и `imports`.

Для уже развёрнутого PostgreSQL 18 Compose явно сохраняет прежний `PGDATA=/var/lib/postgresql/data`, чтобы redeploy продолжил использовать существующий named volume. Не меняйте mount на `/var/lib/postgresql` без предварительного dump/restore: это может привести к запуску с пустым кластером. Для нового layout PostgreSQL 18 переход выполняется отдельной обслуживающей операцией после проверки резервной копии.

Перед обновлением стека сделайте резервные копии PostgreSQL и MinIO. Смена значения `POSTGRES_PASSWORD` в Portainer не меняет пароль уже созданной роли PostgreSQL: сначала измените пароль в самой БД, затем синхронно обновите переменную окружения.

Секреты `POSTGRES_PASSWORD`, `JWT_SECRET`, `MINIO_ROOT_PASSWORD`, токены Dashboard и ключ OpenAI нельзя оставлять со значениями `change-me-*`. Если секрет был опубликован в чате, issue, логе или истории команд, его следует отозвать и выпустить заново.

Долгоживущие контейнеры имеют restart policy и ротацию Docker JSON-логов. API и web используют health checks; web стартует только после успешной проверки API. Для более строгого production-контура образы приложения следует собирать в CI и публиковать в registry с неизменяемыми тегами, но для pet-проекта допустима сборка Git-backed стека непосредственно в Portainer.

Конфигурация не предназначена для Portainer Swarm: в Swarm нельзя полагаться на локальный `build` и `depends_on`. Для Swarm нужны заранее собранные образы и отдельная схема запуска одноразовых миграций.
