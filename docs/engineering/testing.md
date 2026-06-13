# Стратегия тестирования

## Структура

Структура тестов повторяет структуру production-проектов:

- корневой проект `src/MenuMate.SharedKernel` проверяется в `tests/MenuMate.SharedKernel.UnitTests`;
- доменный проект `src/Modules/<Module>/MenuMate.Modules.<Module>.Domain` проверяется в `tests/Modules/<Module>/MenuMate.Modules.<Module>.Domain.UnitTests`;
- внутри unit-test проекта каталоги `Models`, `ValueObjects`, `Services` и другие повторяют каталоги тестируемого проекта;
- API-интеграционные тесты находятся в `tests/MenuMate.Api.IntegrationTests/Modules/<Module>`;
- общая инфраструктура интеграционных тестов находится в `tests/MenuMate.Api.IntegrationTests/Infrastructure`.

Не следует создавать пустой test-проект только ради симметрии. Отдельный проект создается, когда в production-проекте появляется поведение, которое имеет смысл проверять изолированно.

## Доменные unit-тесты

Unit-тестами покрываются:

- инварианты создания доменных моделей и value objects;
- допустимые и недопустимые переходы состояния;
- нормализация и дедупликация пользовательских значений;
- масштабирование, группировка, преобразование единиц и другие чистые доменные расчеты;
- поведение базовых типов SharedKernel.

Простые DTO, перечисления без поведения и persistence-конфигурации не требуют unit-тестов.

## API-интеграционные тесты

Интеграционные тесты поднимают API через `WebApplicationFactory`, используют настоящие обработчики, PostgreSQL в Testcontainers и применяют EF-миграции.

Основные сценарии:

- Auth: регистрация, вход, профиль, refresh/logout, ошибки учетных данных и уникальности email;
- Recipes: жизненный цикл собственного рецепта, фильтры, избранное, права доступа, библиотека и копирование;
- Products: создание продуктов через пользовательские сценарии, поиск и варианты одинакового названия в разных категориях;
- Tags: создание, поиск, подтверждение, скрытие и уникальность;
- MenuPlanning: приемы пищи, позиции календаря, очистка диапазона и изоляция пользователей;
- ShoppingLists: предпросмотр меню, генерация единственного списка, ручные позиции, отметки и замена списка.

## Запуск

Полный набор:

```bash
dotnet test MenuMate.slnx --no-restore
```

Только доменные unit-тесты конкретного модуля:

```bash
dotnet test tests/Modules/Recipes/MenuMate.Modules.Recipes.Domain.UnitTests/MenuMate.Modules.Recipes.Domain.UnitTests.csproj
```

Только интеграционный сценарий:

```bash
dotnet test tests/MenuMate.Api.IntegrationTests/MenuMate.Api.IntegrationTests.csproj --filter FullyQualifiedName~RecipeWorkflowTests
```
