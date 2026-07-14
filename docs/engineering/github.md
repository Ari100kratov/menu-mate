# GitHub Actions и Dependabot

## Continuous integration

`.github/workflows/ci.yml` содержит два независимых required-check кандидата:

- `Backend` — restore, Release build и все проекты `*.UnitTests.csproj`;
- `Frontend` — frozen pnpm install, ESLint, Prettier и production build.

Workflow имеет минимальное разрешение `contents: read`, отменяет устаревший запуск для той же ветки и ограничен таймаутами. API-интеграционные тесты в CI не запускаются; причина и локальные команды описаны в [стратегии тестирования](testing.md).

В настройках GitHub рекомендуется включить ruleset для `main`:

- запрет прямого push;
- pull request перед merge;
- обязательные проверки `Backend` и `Frontend`;
- актуальная ветка перед merge;
- запрет force push и удаления ветки;
- автоматическое удаление merged branches.

В разделе security также следует включить secret scanning и push protection, если они доступны для репозитория. Секрет, однажды попавший в commit или лог CI, считается скомпрометированным и должен быть отозван; удаления из текущей версии файла недостаточно.

Ruleset хранится в настройках репозитория и не может быть надёжно задан обычным файлом без знания владельца и тарифа GitHub.

## Dependabot

`.github/dependabot.yml` каждую неделю по понедельникам проверяет:

- NuGet и central package management;
- npm/pnpm во frontend;
- Docker Compose и все Dockerfile;
- версии GitHub Actions.

Minor- и patch-обновления NuGet/frontend группируются, чтобы не создавать шум. Major-обновления остаются отдельными pull request и требуют ручной проверки release notes и миграционных заметок. Dependabot PR проходят тот же CI, что и остальные изменения.

Автоматический merge намеренно не включён: обновления EF Core, Aspire, React, Vite, OpenAI SDK и базовых Docker-образов могут требовать изменения кода, миграции данных или ручной проверки deployment.

## Pull request

Шаблон `.github/pull_request_template.md` напоминает приложить описание риска, локальные проверки, изменения документации/API-контракта и скриншоты для UI. Неприменимые пункты можно отметить пояснением, а не запускать формально.
