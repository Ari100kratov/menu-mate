# MenuMate

MenuMate — mobile-first приложение для личной библиотеки рецептов, календаря питания и единого списка покупок.

Проект построен как модульный монолит: ASP.NET Core API и React SPA развиваются в одном репозитории, а бизнес-модули разделены проектами, PostgreSQL-схемами и явными контрактами.

## Что уже работает

- регистрация, вход, обновление и отзыв сессии;
- личные и публичные рецепты, неизменяемые ревизии, библиотека, избранное и копирование;
- общий каталог продуктов, категории блюд и теги;
- обложки и изображения шагов в MinIO;
- импорт рецепта из нескольких изображений через OpenAI-совместимый API с обязательной проверкой черновика;
- единый календарь питания с настраиваемыми приёмами пищи;
- предпросмотр покупок по диапазону меню и единый редактируемый список;
- mobile-first SPA с нижней навигацией на телефоне и боковой панелью на широком экране;
- отдельный мигратор БД, OpenTelemetry и Aspire Dashboard.

## Стек

- .NET 10, ASP.NET Core Minimal API, EF Core, PostgreSQL 18;
- .NET Aspire для локальной оркестрации;
- React 19, TypeScript 5, Vite 8, Tailwind CSS 4, shadcn/ui;
- TanStack Query и Form, Zustand, Zod;
- MinIO для изображений;
- xUnit, Testcontainers и GitHub Actions.

## Быстрый старт

Понадобятся .NET SDK 10, Node.js 22, pnpm 11 через Corepack и Docker-совместимый container runtime.

Сначала установите зависимости фронтенда:

```powershell
corepack enable
cd src/MenuMate.Web
pnpm install --frozen-lockfile
cd ../..
```

Затем запустите Aspire AppHost:

```powershell
dotnet run --project src/MenuMate.AppHost/MenuMate.AppHost.csproj
```

AppHost поднимает PostgreSQL, MinIO, инициализацию бакетов, мигратор, API и Vite. Адреса приложения, API и инфраструктуры отображаются в Aspire Dashboard. OpenAI нужен только для распознавания рецептов и генерации обложек; без ключа остальные сценарии доступны.

API публикует:

- OpenAPI: `/openapi/v1.json`;
- Scalar: `/scalar/v1`;
- health check: `/health`.

OpenAPI и Scalar включены в Development. В production они по умолчанию отключены и временно включаются через `EXPOSE_API_DOCS=true`.

## Проверки

Backend:

```powershell
dotnet restore MenuMate.slnx
dotnet build MenuMate.slnx --configuration Release --no-restore
dotnet test MenuMate.slnx --configuration Release --no-build
```

Последняя команда запускает и unit-, и интеграционные тесты; для интеграционных тестов нужен Docker. Команды только для unit-тестов и правила CI описаны в [стратегии тестирования](docs/engineering/testing.md).

Frontend:

```powershell
cd src/MenuMate.Web
pnpm lint
pnpm format
pnpm build
```

При изменении API-контрактов дополнительно выполните `pnpm api:generate` и закоммитьте обновлённый `src/shared/api/generated/schema.d.ts`.

## Структура

```text
src/
  MenuMate.Api/             composition root и HTTP API
  MenuMate.AppHost/         локальная Aspire-оркестрация
  MenuMate.Migrator/        применение всех EF Core миграций
  MenuMate.DataImporter/    административный импорт из Wikibooks
  MenuMate.Web/             React SPA
  MenuMate.Contracts/       публичные DTO
  MenuMate.SharedKernel/    общие доменные примитивы
  MenuMate.Common.*/        общие application/infrastructure/presentation типы
  Modules/                  бизнес-модули
tests/                      unit- и API-интеграционные тесты
docs/                       архитектура, инженерные правила и ADR
```

## Развёртывание и документация

Production-стек на Docker Compose описан в [руководстве по развёртыванию](docs/architecture/deployment.md); пример обязательных переменных находится в [.env.example](.env.example).

Начальная точка документации — [docs/README.md](docs/README.md). Там собраны архитектура, UX-решения, тестирование, правила GitHub и индекс ADR.
