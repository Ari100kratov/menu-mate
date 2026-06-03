# 0012 Frontend: генерация API и auth-flow

## Статус

Принято.

## Контекст

Фронтенду нужен типобезопасный доступ к backend API без ручного дублирования контрактов. При этом refresh-token нельзя хранить в `localStorage` или другом JavaScript-доступном хранилище.

## Решение

Фронтенд живет в `src/MenuMate.Web` и использует Vite, React, TypeScript, Tailwind, shadcn/ui, TanStack Query, TanStack Form и Zustand.

Типы API генерируются из backend OpenAPI через `openapi-typescript`. Запросы выполняются через `openapi-fetch`, а feature-слой вручную описывает query hooks и UX-состояния.

TypeScript фиксируется на совместимой ветке 5.x, потому что `openapi-typescript` 7.x требует `typescript ^5.x`.

Auth-flow:

- access-token хранится только в памяти;
- refresh-token выставляется backend-ом в `HttpOnly` cookie;
- refresh endpoint читает cookie, ротирует refresh-token и возвращает только новый access-token;
- общий API-клиент один раз повторяет запрос после успешного refresh при `401`.

## Последствия

Backend OpenAPI должен оставаться пригодным для генерации: endpoint-ы обязаны описывать success response, `ProblemDetails` и security metadata.

Фронтенд не редактирует файлы в `src/shared/api/generated` вручную. Любое изменение контрактов проходит через обновление backend OpenAPI и повторный `pnpm api:generate`.

Формы строятся через TanStack Form + Zod. Ручное управление большим набором `useState` для форм не используется.

Генератор OpenAPI запускает backend с отдельным `MENUMATE_DATA_PROTECTION_KEYS_PATH`, чтобы не зависеть от пользовательских DPAPI-ключей Windows.

CRUD feature-срезы строятся вокруг сгенерированных контрактов: `features/<feature>/api` содержит тонкие вызовы `openapi-fetch`, `features/<feature>/api/*.queries.ts` содержит TanStack Query hooks, а `features/<feature>/ui` содержит формы и виджеты. Detail-response API должен возвращать все поля, которые форма может редактировать. Иначе повторное сохранение формы способно стереть поле, отсутствующее в ответе.
