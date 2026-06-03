# Документация MenuMate

Документация нужна, чтобы разработчик или AI-агент мог продолжить работу без восстановления архитектуры из переписки.

## Читать сначала

1. [Обзор архитектуры](architecture/overview.md)
2. [Модули](architecture/modules.md)
3. [Бэкенд](architecture/backend.md)
4. [Фронтенд](architecture/frontend.md)
5. [Развертывание](architecture/deployment.md)
6. [ADR](adr/README.md)
7. [Правила backend-разработки](engineering/backend-guardrails.md)

## Текущий срез

- Бэкенд собирается на .NET 10.
- Реализованы доменные правила Recipes, Tags, MenuPlanning и ShoppingLists.
- API публикует конечные точки Auth, Recipes, Tags, MenuPlanning и ShoppingLists.
- Auth, Recipes, Tags, MenuPlanning и ShoppingLists используют отдельные схемы PostgreSQL и собственные DbContext.
- `MenuMate.Migrator` применяет миграции до старта API в Aspire и Docker Compose.
- MinIO подключен как общее объектное хранилище для изображений.
- Scalar доступен во всех окружениях.
- Фронтенд пока описан, но не создан.

## Следующий рекомендуемый срез

1. Добавить интеграционные тесты auth-required workflow.
2. Спроектировать семейные пространства как расширение модели владения.
3. Добавить модуль Imports с черновиками импорта из URL и изображений.
4. Добавить модуль Files для загрузки и проверки изображений через MinIO.
