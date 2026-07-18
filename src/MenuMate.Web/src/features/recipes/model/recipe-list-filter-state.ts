import { useCallback } from "react"

import { usePersistentState } from "@/shared/lib/persistent-state"

export type RecipeListScope = "library" | "catalog"

export interface RecipeListTagFilter {
  id: string
  name: string
}

export interface RecipeListFilterValues {
  search: string
  category: string
  tags?: RecipeListTagFilter[]
  // Поддержка сохранённого одиночного фильтра до перехода на множественный выбор.
  tagId?: string
  // Поддержка сохранённого одиночного фильтра до перехода на множественный выбор.
  tagName?: string
  favoritesOnly: boolean
}

interface RecipeListFilterState {
  scope: RecipeListScope
  filters: RecipeListFilterValues
}

interface LegacyRecipeListFilterState {
  scope: RecipeListScope
  filters: Record<RecipeListScope, RecipeListFilterValues>
}

type StoredRecipeListFilterState = RecipeListFilterState | LegacyRecipeListFilterState

const emptyFilters: RecipeListFilterValues = {
  search: "",
  category: "",
  tags: [],
  tagId: "",
  tagName: "",
  favoritesOnly: false,
}

const initialState: RecipeListFilterState = {
  scope: "library",
  filters: { ...emptyFilters },
}

export function useRecipeListFilterState(storageKey: string) {
  const [storedState, setStoredState] = usePersistentState<StoredRecipeListFilterState>(
    storageKey,
    initialState,
    isStoredRecipeListFilterState,
  )
  const state = normalizeState(storedState)
  const selectedTags = getSelectedTags(state.filters)

  const setScope = useCallback(
    (scope: RecipeListScope) => {
      setStoredState((current) => ({ ...normalizeState(current), scope }))
    },
    [setStoredState],
  )

  const updateFilters = useCallback(
    (updates: Partial<RecipeListFilterValues>) => {
      setStoredState((current) => {
        const normalized = normalizeState(current)
        return {
          ...normalized,
          filters: {
            ...normalized.filters,
            ...updates,
          },
        }
      })
    },
    [setStoredState],
  )

  const resetFilters = useCallback(() => {
    updateFilters(emptyFilters)
  }, [updateFilters])

  return {
    scope: state.scope,
    ...state.filters,
    selectedTags,
    setScope,
    setSearch: (search: string) => {
      updateFilters({ search })
    },
    setCategory: (category: string) => {
      updateFilters({ category })
    },
    setTags: (tags: RecipeListTagFilter[]) => {
      updateFilters({ tags, tagId: undefined, tagName: undefined })
    },
    setFavoritesOnly: (favoritesOnly: boolean) => {
      updateFilters({ favoritesOnly })
    },
    resetActiveFilters: resetFilters,
  }
}

function normalizeState(state: StoredRecipeListFilterState): RecipeListFilterState {
  if (isLegacyRecipeListFilterState(state)) {
    return {
      scope: state.scope,
      filters: state.filters[state.scope],
    }
  }

  return state
}

function isStoredRecipeListFilterState(value: unknown): value is StoredRecipeListFilterState {
  return isRecipeListFilterState(value) || isLegacyRecipeListFilterState(value)
}

function isRecipeListFilterState(value: unknown): value is RecipeListFilterState {
  return isStateShell(value) && isFilterValues(value.filters)
}

function isLegacyRecipeListFilterState(value: unknown): value is LegacyRecipeListFilterState {
  return (
    isStateShell(value) &&
    isRecord(value.filters) &&
    isFilterValues(value.filters.library) &&
    isFilterValues(value.filters.catalog)
  )
}

function isStateShell(value: unknown): value is { scope: RecipeListScope; filters: unknown } {
  return isRecord(value) && (value.scope === "library" || value.scope === "catalog")
}

function isFilterValues(value: unknown): value is RecipeListFilterValues {
  return (
    isRecord(value) &&
    typeof value.search === "string" &&
    typeof value.category === "string" &&
    (value.tags === undefined ||
      (Array.isArray(value.tags) && value.tags.every(isRecipeListTagFilter))) &&
    (value.tagId === undefined || typeof value.tagId === "string") &&
    (value.tagName === undefined || typeof value.tagName === "string") &&
    typeof value.favoritesOnly === "boolean"
  )
}

function getSelectedTags(filters: RecipeListFilterValues) {
  const storedTags = filters.tags ?? []
  const tags =
    storedTags.length > 0
      ? storedTags
      : filters.tagId && filters.tagName
        ? [{ id: filters.tagId, name: filters.tagName }]
        : []

  return tags.filter(
    (tag, index) => tag.id.length > 0 && tags.findIndex((item) => item.id === tag.id) === index,
  )
}

function isRecipeListTagFilter(value: unknown): value is RecipeListTagFilter {
  return (
    isRecord(value) &&
    typeof value.id === "string" &&
    value.id.length > 0 &&
    typeof value.name === "string" &&
    value.name.length > 0
  )
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null
}
