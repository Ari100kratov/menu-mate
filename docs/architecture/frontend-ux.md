# Frontend UX decisions

## Navigation shell

MenuMate uses a mobile-first app shell:

- Top app bar owns the current screen title.
- Child/detail screens use a compact icon-only Back action in the top app bar.
- Breadcrumbs are not shown in the mobile shell. They duplicate the Back action, compete with the title, and cost too much vertical space on narrow screens.
- Bottom navigation is reserved for frequent workspace sections only: Recipes, Menu, Shopping.
- Profile is an account action, not a primary workspace section. It lives in the top app bar on mobile and in the account area/sidebar on desktop.

Desktop keeps the sidebar for workspace navigation, but the top app bar remains the single place for the current page title so mobile and desktop use the same mental model.

## Page title placement

Route-level page titles must not be repeated as large headers inside the working area. The page content starts with the most relevant action, filters, form, or workspace. Section titles inside cards/forms are still allowed when they describe a local subsection rather than the whole route.

## Recipe create/edit flow

Recipe create and edit are one vertically ordered form:

- Cover
- About the recipe
- Ingredients
- Steps
- Additional

The primary sections stay visible on one page. Tabs hid the overall recipe structure, made review before saving difficult, and required unnecessary context switching on mobile. The form remains a single TanStack Form instance and submits one recipe DTO.

Ingredients are rendered as compact summary rows. Adding or editing an ingredient opens a focused mobile bottom sheet / desktop dialog, because displaying every ingredient field inline makes the recipe unreadably long. The dialog edits an isolated draft and updates the recipe only after successful validation and explicit confirmation; canceling never leaves an empty or partially edited row behind. Steps remain inline because their short ordered text is useful while reviewing the full recipe.

The ingredient and steps sections keep one full-width add action after the final item. Add actions stay in normal document flow and do not float above the recipe editor. Header actions are intentionally omitted so section titles remain clean.

Cooking duration is entered as separate hour and minute segments for total and active time. Raw minute-only number fields are not exposed to users.

Cover selection is part of the main editor. On create it is staged locally and uploaded after the recipe record is created; on edit it replaces the current cover after saving recipe data.

Repeatable sections keep add actions after the final item. Save remains sticky near the bottom of the viewport, with a small gap above the mobile bottom navigation and normal-flow spacing after the final form section. Tags and source URL are secondary fields and live in a collapsed Additional section.

Submitting an invalid recipe shows a neutral guidance toast and moves focus to the first invalid field. Compact ingredient rows also show their first validation message and an invalid border, even while their editing dialog is closed. Successful create and update operations also show a toast so save state is never communicated only through button loading state.

Ingredient validation also runs inside the focused editor. Invalid controls are highlighted, the editor scrolls to the first issue, and focus moves there without closing the dialog.

Validation styling is consistent across forms: labels keep their normal foreground color, invalid controls receive a destructive border/ring, and validation messages use destructive text. Invalid state must not recolor an entire field container.

## Product selection

Recipes and shopping lists use the same product catalog:

- Selecting an existing product fills its category automatically.
- The selected catalog category stays visible in the ingredient editor. Product category is editable only while creating a new catalog product.
- New product names are normalized and added to the shared catalog on save.
- Recipe ingredients and shopping list items keep a product identifier so aggregation does not depend on matching free-form text.
- Quantity kind is a compact three-option control paired with amount and unit inputs.
- Ingredient preparation notes are displayed as supporting descriptions in both edit and read views.
- Optional ingredients use the explicit wording "Можно пропустить" with a dedicated visual badge rather than being appended as comma-separated text.
- Ingredient quantity and product category are shown together in both edit and read summaries.

## Recipe read flow

Recipe details use one continuous read view: overview, ingredients, and cooking steps. Tabs are not used because they hide recipe content and differ from the compact reference flow.

Recipe detail actions are compact icon buttons beside the title, with accessible names and tooltips. Add-to-menu is not exposed from the recipe read view. Destructive deletion always uses an explicit confirmation dialog rather than a browser-native prompt.

The description sits directly below the recipe title. Recipe facts use a two-column labeled icon grid on all viewport sizes, the source is shown after the facts, and actual user tags are displayed separately without decorative prefixes.

## Recipe images

Cover image management is integrated into recipe create/edit. Step image management is hidden for now in the UI even though the backend contract supports it. This avoids presenting a half-polished workflow while the recipe form and cooking view are being stabilized.

## Recipe list

The recipe list prioritizes scanning:

- Search is the primary control.
- Recipe categories use a horizontally scrollable chip row that supports touch, trackpad, and a desktop mouse wheel.
- Favorites use a compact heart toggle next to search instead of consuming category-row space.
- Tags are not exposed as a primary list filter.
- Recipe items use compact horizontal image/title/metadata rows. The title is the primary line; category, total time, active time, and servings are secondary metadata.
- Edit and delete actions live on the recipe detail page rather than every list row.

## UI primitives

Base components in `src/MenuMate.Web/src/shared/ui` must come from the shadcn/ui registry when a registry component exists. Use `pnpm dlx shadcn@latest add <component>` from `src/MenuMate.Web`, then compose app-specific UX on top of the generated primitive.

## Typography

MenuMate uses a small semantic type scale:

- `type-page-title`: route title, 18px on mobile and 20px on desktop.
- `type-recipe-title`: recipe name on the read screen, 24px.
- `type-section-title`: major content section or dialog title, 18px.
- `type-subsection-title`: card title or nested group title, 16px semibold.
- `type-body`: primary content and long-form text, 16px with 24px line height.
- `type-label` and `type-supporting`: field labels, metadata, descriptions, and errors, 14px with 20px line height.

Form control values stay at 16px on every viewport. This keeps controls consistent and prevents automatic zoom on iOS. The 12px size is reserved for constrained navigation and dense calendar-like interfaces, not recipe content or form guidance.

## Recipe library and sharing

- The recipe list has two explicit scopes: the user's library and the public catalog.
- The library/catalog scope selector uses a high-contrast segmented control with an explicit selected state in both themes.
- New recipes are public by default; the editor explains that only the owner can change the original recipe.
- Only owned recipes expose edit and delete actions.
- Public recipes can be saved to the library or copied into a new private owned recipe.
- Adding a recipe to a menu sends both the recipe ID and its current immutable revision ID.
