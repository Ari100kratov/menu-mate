# Обзор архитектуры

MenuMate — модульный монолит с ASP.NET Core API и React SPA. Один deployment и одна PostgreSQL-инсталляция сохраняют локальную разработку и эксплуатацию простыми, а границы модулей уже выражены отдельными проектами, схемами БД и контрактами.

## Компоненты

```text
Browser
  -> MenuMate.Web (React SPA / Nginx)
      -> MenuMate.Api
          -> Auth, Recipes, Products, Tags,
             MenuPlanning, ShoppingLists, RecipeImports
          -> PostgreSQL
          -> MinIO
          -> OpenAI-compatible API (только AI-сценарии)

MenuMate.AppHost -> локальная оркестрация
MenuMate.Migrator -> миграции всех DbContext до старта API
MenuMate.DataImporter -> отдельная административная операция
```

В production встроенный Nginx отдаёт SPA, проксирует `/api`, а также подписанные ссылки бакетов MinIO. В локальной разработке те же зависимости запускает Aspire AppHost, а Vite проксирует API.

## Принципы

- Доменная логика не живёт в конечных точках API и React-компонентах.
- Модули общаются через публичные контракты, прикладные сценарии и явные read-модели.
- Stateful-модуль владеет своим DbContext и PostgreSQL-схемой.
- Межмодульные связи используют типизированные идентификаторы, например `UserId`, `RecipeId` и `RecipeRevisionId`.
- Данные другого модуля читаются как contract/snapshot, а не через прямую мутацию чужих сущностей.
- API является источником истины; frontend-состояние не дублирует доменные правила.
- Миграции, документация и проверки CI являются частью изменения.

## Форма бэкенда

```text
MenuMate.Api
MenuMate.Contracts
MenuMate.SharedKernel
MenuMate.Common.*
Modules/<ModuleName>/
  MenuMate.Modules.<ModuleName>.Domain
  MenuMate.Modules.<ModuleName>.Application
  MenuMate.Modules.<ModuleName>.Infrastructure
  MenuMate.Modules.<ModuleName>.Presentation
```

Не каждый модуль обязан иметь все четыре слоя: например, Products сейчас не содержит отдельные Domain и Application проекты, потому что его поведение сосредоточено в каталоге инфраструктуры и presentation-контракте.

## Владение данными

Одна база PostgreSQL разделена на схемы `auth`, `recipes`, `products`, `tags`, `menu_planning`, `shopping_lists`, `imports` и служебную `data_import`. Все миграции применяет `MenuMate.Migrator` до запуска API.

Изображения рецептов находятся в бакете MinIO `images`; исходники черновиков импорта — в закрытом бакете `imports`. В БД хранятся только метаданные и object key.

## Импорт рецептов

Пользовательский импорт из изображений всегда создаёт редактируемый черновик в RecipeImports. Рецепт появляется в Recipes только после явного подтверждения пользователя.

Отдельный `MenuMate.DataImporter` служит для первоначального административного наполнения публичными рецептами из Wikibooks и не входит в обычный runtime приложения.
