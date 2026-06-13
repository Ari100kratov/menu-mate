# Развертывание

Планируемый deployment — Linux host под Docker/Portainer.

## Локальная инфраструктура

Основной локальный запуск — Aspire:

```powershell
dotnet run --project src/MenuMate.AppHost/MenuMate.AppHost.csproj
```

AppHost поднимает PostgreSQL, MinIO, создает публичный read-only бакет `images`, запускает `MenuMate.Migrator`, ждет его завершения и затем стартует API.

`docker-compose.yml` остается scaffold для deployment и содержит PostgreSQL, MinIO, one-shot `minio-init`, one-shot migrator и API.

```powershell
copy .env.example .env
docker compose up -d --build
```

## Направление для production

- reverse proxy перед frontend/API;
- PostgreSQL volume backups;
- MinIO volume backups;
- secrets через environment variables или secret manager;
- API health checks;
- миграции через `MenuMate.Migrator`, а не скрыто внутри API startup;
- публичный адрес MinIO API для чтения изображений фронтом.

## Пока не реализовано

- frontend Dockerfile;
- production compose override;
- политика lifecycle/backup для MinIO bucket.
