## Frontend UI

- Do not hand-roll base UI primitives in `src/MenuMate.Web/src/shared/ui` when a shadcn/ui registry component exists.
- Add or update shadcn-managed primitives with `pnpm dlx shadcn@latest add <component>` from `src/MenuMate.Web` and keep the generated component API.
- Local files in `src/shared/ui` may wrap or compose shadcn primitives for app-specific patterns, but they must not replace registry primitives such as `button`, `input`, `select`, `textarea`, `label`, `field`, `alert`, `skeleton`, or `sonner`.
- Record important frontend architecture and UX decisions in `docs/architecture`, especially navigation, mobile layout, form flow, and design-system rules.
- Write Russian user-facing frontend copy with `е` instead of `ё`. This applies only to visible interface text, not API data, search normalization, or internal comments.

## Documentation language

- Write project documentation in Russian. This applies to `docs/**`, ADRs, architecture documents, README-style project documentation, and UX decisions.
- Agent-only instruction files such as `AGENTS.md` may remain in English.
