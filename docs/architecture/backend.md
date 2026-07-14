# Архитектура бэкенда

## Стек

- .NET 10 и ASP.NET Core Minimal API;
- EF Core и PostgreSQL 18;
- Aspire AppHost и `MenuMate.ServiceDefaults`;
- OpenAPI и Scalar;
- JWT bearer-аутентификация;
- MinIO для изображений;
- OpenTelemetry для логов, метрик и трейсов;
- xUnit и Testcontainers.

## Shared Kernel и Common

`MenuMate.SharedKernel` содержит только стабильные примитивы: `Result<T>`, `AppError`, базовую сущность, marker доменных событий, нормализацию текста и типизированные межмодульные идентификаторы. Feature-specific helpers сюда не добавляются.

- `MenuMate.Common.Application` — CQRS-абстракции, `IUserContext` и общие application-контракты.
- `MenuMate.Common.Presentation` — преобразование результатов в HTTP и единый RFC 7807 `ProblemDetails`.
- `MenuMate.Common.Infrastructure` — общие интеграции с инфраструктурой, включая MinIO и кеширование подписанных ссылок.
- `MenuMate.Common.Domain` — место только для действительно общих доменных типов; сейчас оно намеренно минимально.

## Правила API

- Endpoint остаётся тонким и передаёт работу application-handler.
- Доменные инварианты находятся в Domain-проектах.
- Группы пользовательских endpoint’ов требуют авторизацию по умолчанию.
- Доменные, validation-, auth- и infrastructure-ошибки возвращаются как `ProblemDetails`.
- OpenAPI содержит success/error schemas и security metadata, потому что из него генерируются frontend-типы.
- В Development OpenAPI доступен по `/openapi/v1.json`, Scalar — по `/scalar/v1`.
- В production документация API отключена; временное включение управляется `Diagnostics:ExposeApiDocs` или `EXPOSE_API_DOCS=true` в Compose.

## Структура модулей

Domain-проекты при наличии соответствующего поведения используют каталоги `Models`, `ValueObjects`, `Enums`, `Errors`, `Services`. Persistence-типы находятся в `Database/Entities`, конфигурации EF Core — в `Database/Configurations` и подключаются через `ApplyConfigurationsFromAssembly`.

Командный сценарий может использовать Repository и Unit of Work, когда нужно загрузить агрегат и применить инварианты. Запросы чтения предпочитают проекцию через read DbContext-интерфейс и не гидрируют агрегат без необходимости.

## Persistence и миграции

Stateful-компоненты владеют отдельными DbContext и схемами:

- Auth — `AuthDbContext`, `auth`;
- Recipes — `RecipesDbContext`, `recipes`;
- Products — `ProductsDbContext`, `products`;
- Tags — `TagsDbContext`, `tags`;
- MenuPlanning — `MenuPlanningDbContext`, `menu_planning`;
- ShoppingLists — `ShoppingListsDbContext`, `shopping_lists`;
- RecipeImports — `RecipeImportsDbContext`, `imports`;
- DataImporter — `DataImportDbContext`, `data_import`.

`MenuMate.Migrator` применяет миграции всех этих контекстов. Aspire и Docker Compose ждут успешного завершения мигратора до старта API.

## Владение рецептами и ревизии

- У рецепта один владелец; только он редактирует, удаляет рецепт и управляет изображениями.
- Запись рецепта является текущей проекцией, а каждое сохранение содержимого добавляет неизменяемую ревизию.
- Приватный рецепт доступен владельцу, публичный можно читать и сохранять в библиотеку.
- Изменение чужого рецепта создаёт новую приватную копию со ссылкой на источник.
- Позиция меню фиксирует ревизию, а список покупок читает ингредиенты именно этой ревизии.

## Авторизация

Пароли хешируются PBKDF2. API выпускает короткоживущий JWT access token и ротируемый opaque refresh token. Refresh token хранится в PostgreSQL и передаётся только через cookie `MenuMate.RefreshToken` с `HttpOnly`, `Secure` и `SameSite=Lax`.

Текущий пользователь доступен application-коду через `IUserContext`. Модули фильтруют чтение по владельцу и проверяют владение до мутации.

## Файлы и изображения

Backend является единственной точкой добавления и удаления изображений: он проверяет владельца, MIME/signature и размер, сохраняет объект в MinIO и метаданные в БД.

Бакеты:

- `images` — обложки и изображения шагов рецептов;
- `imports` — закрытые исходники черновиков RecipeImports.

Ключи рецептов имеют форму:

```text
users/{ownerUserId:N}/recipes/{recipeId:N}/images/cover/{imageId:N}.{extension}
users/{ownerUserId:N}/recipes/{recipeId:N}/images/steps/{stepNumber}/{imageId:N}.{extension}
```

Клиент читает объект по `readUrl`, который возвращает API. Обычная выдача бинарных данных через API не проксируется. В БД нельзя сохранять произвольный URL как изображение сущности: сохраняются `BucketName`, `ObjectKey`, scope и технические метаданные.

Для рецепта поддерживается одна активная обложка и одно активное изображение на шаг. Metadata изменяется транзакционно, а удаление старого объекта выполняется после сохранения; сбой очистки логируется и не откатывает уже завершённую пользовательскую операцию.

## Наблюдаемость

`MenuMate.ServiceDefaults` подключает health checks, service discovery и OpenTelemetry. API и мигратор отправляют OTLP-телеметрию в Aspire Dashboard. Прикладной код зависит от стандартного `ILogger`, без обязательной привязки к Serilog.

## Проверки

Сборка выполняется с `TreatWarningsAsErrors`, последним analysis level и code style в build. Структура unit- и интеграционных тестов, а также отличие локального полного запуска от CI описаны в [стратегии тестирования](../engineering/testing.md).
