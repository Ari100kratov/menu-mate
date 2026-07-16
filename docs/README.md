# Документация MenuMate

Документация описывает текущее устройство проекта. Исторические причины решений сохраняются отдельно в ADR; планы и backlog здесь намеренно не дублируются.

## Начать отсюда

1. [Обзор архитектуры](architecture/overview.md)
2. [Модули и владение данными](architecture/modules.md)
3. [Архитектура бэкенда](architecture/backend.md)
4. [Архитектура фронтенда](architecture/frontend.md)
5. [UX-решения фронтенда](architecture/frontend-ux.md)
6. [Развёртывание](architecture/deployment.md)

## Сценарии и интеграции

- [Версионирование, копии и жизненный цикл рецептов](architecture/recipe-versioning.md)
- [Создание рецепта из изображения](architecture/recipe-import-from-image.md)
- [Просмотр изображений рецепта](architecture/recipe-image-viewing.md)
- [Первоначальный импорт рецептов из Wikibooks](architecture/data-importer.md)

## Инженерные правила

- [Стратегия тестирования](engineering/testing.md)
- [Правила backend-разработки](engineering/backend-guardrails.md)
- [GitHub Actions и Dependabot](engineering/github.md)

## Архитектурные решения

[Индекс ADR](adr/README.md) содержит принятые и заменённые решения. ADR фиксируют контекст на момент принятия; актуальные правила разработки находятся в документах `architecture/**` и `engineering/**`.

## Правило обновления

Изменение модульных границ, потоков авторизации, хранения данных, навигации, mobile layout или form flow сопровождается обновлением соответствующего документа. Новое существенное и труднообратимое решение оформляется отдельным ADR.
