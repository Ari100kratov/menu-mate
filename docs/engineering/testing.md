# Стратегия тестирования

## Структура

Тестовые проекты находятся в `tests` и повторяют область production-кода:

- `MenuMate.SharedKernel.UnitTests` — общие доменные примитивы;
- `MenuMate.Common.Infrastructure.UnitTests` — изолируемое поведение общей инфраструктуры;
- `MenuMate.DataImporter.UnitTests` — парсинг, настройки и политики административного импортера;
- `Modules/<Module>/*.UnitTests` — Domain- и Application-поведение конкретного модуля;
- `MenuMate.Api.IntegrationTests` — сквозные HTTP-сценарии модулей.

Пустой test-проект ради симметрии не создаётся. Новый проект появляется, когда у production-проекта есть поведение, которое имеет смысл проверять изолированно.

## Unit-тесты

Unit-тестами покрываются:

- инварианты моделей и value objects;
- допустимые и недопустимые переходы состояния;
- нормализация и дедупликация;
- масштабирование, группировка и преобразование единиц;
- чистые application-сервисы, парсеры и политики;
- поведение базовых типов SharedKernel.

Простые DTO, перечисления без поведения и декларативная EF Core конфигурация отдельного unit-теста не требуют.

## API-интеграционные тесты

`MenuMate.Api.IntegrationTests` поднимает API через `WebApplicationFactory`, запускает PostgreSQL в Testcontainers и применяет настоящие EF Core миграции. Набор проверяет auth, Recipes, Products, Tags, MenuPlanning, ShoppingLists и изоляцию данных пользователей.

Для этих тестов нужен работающий Docker daemon. Они запускаются локально перед изменениями API, persistence, авторизации и межмодульных сценариев, но намеренно исключены из обычного GitHub Actions CI по требованию проекта.

## Frontend

Отдельного frontend test runner сейчас нет. Обязательный quality gate состоит из:

```powershell
cd src/MenuMate.Web
pnpm lint
pnpm format
pnpm build
```

`pnpm build` включает TypeScript typecheck. При изменении backend-контрактов также выполняется `pnpm api:generate`; сгенерированный diff проверяется вместе с ручными frontend-изменениями.

## Локальный запуск

Сначала соберите решение:

```powershell
dotnet restore MenuMate.slnx
dotnet build MenuMate.slnx --configuration Release --no-restore
```

Все тесты, включая интеграционные:

```powershell
dotnet test MenuMate.slnx --configuration Release --no-build
```

Только unit-тесты — тот же отбор проектов, который использует CI:

```powershell
$unitTestProjects = Get-ChildItem tests -Recurse -Filter *.UnitTests.csproj
foreach ($project in $unitTestProjects) {
  dotnet test $project.FullName --configuration Release --no-build --no-restore
}
```

Один проект или сценарий:

```powershell
dotnet test tests/Modules/Recipes/MenuMate.Modules.Recipes.Domain.UnitTests/MenuMate.Modules.Recipes.Domain.UnitTests.csproj
dotnet test tests/MenuMate.Api.IntegrationTests/MenuMate.Api.IntegrationTests.csproj --filter FullyQualifiedName~RecipeWorkflowTests
```

## GitHub Actions

Workflow `.github/workflows/ci.yml` запускается для pull request, push в `main` и вручную. Job `Backend` восстанавливает и собирает всё решение, но выполняет только проекты `*.UnitTests.csproj`; `MenuMate.Api.IntegrationTests` не запускается. Job `Frontend` проверяет lockfile, ESLint, Prettier, TypeScript и production build.

Локальный полный прогон остаётся обязательным для изменений, риск которых не покрывает сокращённый CI.
