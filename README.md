# MenuMate

MenuMate — персональный помощник для рецептов, планирования меню и списков покупок.

## Текущий срез

Реализованы:

- решение .NET 10 в `MenuMate.slnx`;
- модульный монолит с явными границами модулей;
- Aspire AppHost для локального запуска;
- PostgreSQL, MinIO, мигратор и API в локальной оркестрации;
- Scalar UI и OpenAPI, доступные во всех окружениях;
- фронтенд `src/MenuMate.Web` на Vite, React, TypeScript и Tailwind;
- mobile-first shell: нижняя навигация на телефоне и sidebar на desktop;
- TanStack Query для серверного состояния, Zustand для состояния auth-сессии;
- shadcn/ui-подход: компоненты добавляются локально в `shared/ui`;
- auth-flow с access-token в памяти и refresh-token в `HttpOnly` cookie;
- модули Auth, Recipes, Tags, MenuPlanning и ShoppingLists;
- изображения рецептов через MinIO: фронт читает `readUrl`, загрузка и удаление идут через backend;
- теги как inline-сценарий рецептов, без отдельного раздела в основной навигации MVP;
- недельный план меню и генерация списка покупок из плана;
- список покупок с прогрессом, фильтрами, inline-редактированием позиций через `PUT`, массовыми действиями по категориям и текущей выборке, а также sticky-действием в режиме похода в магазин;
- профиль с темой интерфейса, данными пользователя, выходом и локальными предпочтениями для ручных покупок;
- PostgreSQL-схемы и EF Core миграции для Auth, Recipes, Tags, MenuPlanning и ShoppingLists;
- доменные тесты для рецептов, меню и списков покупок;
- архитектурная документация и ADR.

## Mobile-first правило

Каждый новый пользовательский сценарий проектируется сначала для телефона:

- основные разделы доступны через нижнюю навигацию;
- рабочие экраны состоят из коротких блоков, пригодных для одной руки;
- таблицы и широкие сетки не используются как обязательный мобильный интерфейс;
- desktop получает расширенный layout поверх той же логики, а не отдельный продуктовый поток;
- теги, админские справочники и вторичные настройки не попадают в основную мобильную навигацию без отдельного продуктового основания.

## Локальные проверки

Backend:

```powershell
dotnet restore MenuMate.slnx
dotnet build MenuMate.slnx --no-restore
dotnet test MenuMate.slnx --no-build
```

Frontend:

```powershell
cd src/MenuMate.Web
pnpm install
pnpm api:generate
pnpm lint
pnpm typecheck
pnpm build
pnpm format
```

Сборка настроена строго: предупреждения анализаторов, code style, TypeScript и ESLint должны проходить без ошибок. Новые подавления правил не добавляются без отдельного решения.

## Локальный запуск через Aspire

Aspire — основной способ локального запуска.

```powershell
dotnet run --project src/MenuMate.AppHost/MenuMate.AppHost.csproj
```

AppHost поднимает PostgreSQL, MinIO, создает бакет `images`, запускает `MenuMate.Migrator`, ждет успешного применения миграций и только после этого запускает API и фронтенд.

## API

- OpenAPI: `/openapi/v1.json`
- Scalar: `/scalar/v1`
- Health: `/health`

Основные текущие endpoint-ы:

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `GET /api/auth/me`
- `POST /api/auth/logout`
- `GET /api/recipes`
- `GET /api/recipes/{id}`
- `POST /api/recipes`
- `PUT /api/recipes/{id}`
- `DELETE /api/recipes/{id}`
- `POST /api/recipes/{id}/favorite`
- `DELETE /api/recipes/{id}/favorite`
- `POST /api/recipes/{id}/images`
- `DELETE /api/recipes/{id}/images/{imageId}`
- `GET /api/tags`
- `POST /api/tags`
- `POST /api/tags/{id}/confirm`
- `DELETE /api/tags/{id}`
- `GET /api/menu-plans`
- `GET /api/menu-plans/{id}`
- `POST /api/menu-plans`
- `PUT /api/menu-plans/{id}`
- `DELETE /api/menu-plans/{id}`
- `POST /api/menu-plans/{id}/items`
- `PUT /api/menu-plans/{id}/items/{itemId}`
- `DELETE /api/menu-plans/{id}/items/{itemId}`
- `GET /api/shopping-lists`
- `GET /api/shopping-lists/{id}`
- `POST /api/shopping-lists`
- `DELETE /api/shopping-lists/{id}`
- `POST /api/shopping-lists/{id}/items`
- `PUT /api/shopping-lists/{id}/items/{itemId}`
- `PATCH /api/shopping-lists/{id}/items/{itemId}/state`
- `DELETE /api/shopping-lists/{id}/items/{itemId}`

Frontend API-типы генерируются из backend OpenAPI через `pnpm api:generate`. Файлы в `src/MenuMate.Web/src/shared/api/generated` вручную не редактируются.

## Структура

```text
src/
  MenuMate.Api/
  MenuMate.AppHost/
  MenuMate.Migrator/
  MenuMate.Web/
  MenuMate.Contracts/
  MenuMate.SharedKernel/
  MenuMate.Common.Application/
  MenuMate.Common.Domain/
  MenuMate.Common.Infrastructure/
  MenuMate.Common.Presentation/
  Modules/
    Auth/
    Recipes/
    Tags/
    MenuPlanning/
    ShoppingLists/
tests/
docs/
```

Доменные проекты дополнительно делятся на `Models`, `ValueObjects`, `Enums`, `Errors`, `Services`, где это имеет смысл. Persistence-типы инфраструктуры лежат в `Database/Entities`, EF Core конфигурации сущностей лежат в `Database/Configurations`.

Фронтенд делится на `app`, `pages`, `features`, `shared/api`, `shared/ui`, `shared/lib` и `shared/config`. Feature-слой пишет query/mutation hooks вручную поверх типизированного API-клиента. Page-компоненты должны оставаться тонким orchestration-слоем; формы, карточки, рабочие области, form hooks, Zod-валидация и form-to-DTO маппинг выносятся в `features/*/ui` и `features/*/model`.

## Дальнейший план

1. Mobile UX hardening: проверить основные экраны на узких viewport, вынести повторяющиеся mobile-first паттерны, улучшить плотность форм, sticky actions и пустые состояния.
2. Рецепты: довести карточки, фильтры, избранное, изображения и детальную страницу до ежедневного сценария; добавить загрузку изображений шагов через существующий `scope=Step`.
3. Меню: улучшить недельную сетку для телефона, добавить быстрые действия по дню и приему пищи, копирование пункта, перенос на другой день и быстрый выбор рецепта.
4. Покупки: MVP-сценарий усилен inline-редактированием, массовыми действиями по категории/текущей выборке и sticky-действием для мобильного режима магазина. Следующий шаг — улучшать UX реального похода в магазин: порядок категорий, скрытие завершенных групп и более явные состояния синхронизации.
5. Профиль и настройки: MVP-сценарий получил явный выбор темы и локальные дефолты единицы/категории для ручных покупок. Следующий шаг — backend-контракт пользовательских настроек, если появится необходимость синхронизации между устройствами.
6. Качество фронтенда: frontend-тесты пока не добавляются; текущий фокус — компактные файлы, тонкие `pages`, feature-компоненты вместо больших экранов, TypeScript/ESLint/format без warnings и ошибок. Крупные recipe/tag/menu компоненты раздроблены; следующий практический шаг — продолжать выносить повторяющиеся field/layout-паттерны по мере появления новых экранов.

## Документация

Начинать лучше с [docs/README.md](docs/README.md), затем читать ADR в [docs/adr](docs/adr).
