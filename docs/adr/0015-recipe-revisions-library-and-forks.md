# ADR 0015: Recipe revisions, user library, and forks

## Status

Accepted.

## Context

A recipe is authored by one user but can be reused by other users in menus and shopping lists. A mutable
`RecipeId` is not sufficient for that workflow: an author edit must not silently change another user's planned
menu or generated shopping list. Favorite state is also user-specific and cannot belong to the recipe aggregate.

Recipe titles are presentation data and are not globally unique.

## Decision

- Every recipe has one owner and `Private` or `Public` visibility.
- Only the owner can edit, delete, or manage images of the recipe.
- Every content save creates an immutable `RecipeRevision` containing the full recipe content snapshot.
- The recipe row remains the current editable projection and points to its current revision.
- Menu items store both `RecipeId` and `RecipeRevisionId`. Shopping lists read ingredients from the pinned revision.
- `RecipeLibraryEntry` stores per-user saved/favorite state. It does not grant edit access.
- A user who wants to change another user's recipe creates a fork. The fork is a new owned recipe with its source
  recipe and source revision recorded.
- Duplicate titles are allowed. Identity and lineage are represented by IDs, not names.

## Consequences

- Author edits are visible when opening the public recipe but do not alter already planned menus.
- Removing or privatizing a recipe does not invalidate immutable revisions already referenced by a user's menu.
- Forks can diverge independently while preserving attribution and future comparison options.
- Discovery and the personal library are separate query scopes.
