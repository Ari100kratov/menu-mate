# 0008 Отдельный мигратор БД

## Статус

Принято.

## Контекст

MenuMate использует модульный монолит. Stateful модули должны владеть DbContext, схемами и миграциями, а API startup должен заниматься обработкой запросов.

Миграции внутри API startup делают порядок deployment неявным и создают риск гонок при нескольких экземплярах API.

## Решение

Создан отдельный executable project `MenuMate.Migrator`.

Мигратор:

- строит host-style service container;
- регистрирует инфраструктуру каждого модуля с состоянием;
- вызывает `Database.MigrateAsync()` для каждого module DbContext;
- запускается Aspire до API;
- представлен one-shot service в Docker Compose.

## Последствия

- API startup не создает и не мигрирует схемы.
- Новый stateful DbContext нужно добавить в migrator.
- Локальный запуск остается простым: Aspire поднимает PostgreSQL, применяет миграции и запускает API.
