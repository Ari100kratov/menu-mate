import { useCallback } from "react"

import { usePersistentState } from "@/shared/lib/persistent-state"

export type RecipeListScope = "library" | "catalog"
export type RecipeListSort = "alphabetical" | "newest" | "popular"
export type RecipeOwnershipFilter = "all" | "mine" | "others"

export interface RecipeListTagFilter {
  id: string
  name: string
}

export interface RecipeListFilterValues {
  search: string
  category: string
  tags?: RecipeListTagFilter[]
  // Поддержка сохраненного одиночного фильтра до перехода на множественный выбор.
  tagId?: string
  // Поддержка сохраненного одиночного фильтра до перехода на множественный выбор.
  tagName?: string
  favoritesOnly: boolean
  sort: RecipeListSort
  ownership: RecipeOwnershipFilter
  showTagsInMain: boolean
}

interface RecipeListFilterState {
  scope: RecipeListScope
  filters: RecipeListFilterValues
}

interface LegacyRecipeListFilterState {
  scope: RecipeListScope
  filters: Record<RecipeListScope, Partial<RecipeListFilterValues>>
}

type StoredRecipeListFilterState = RecipeListFilterState | LegacyRecipeListFilterState

const emptyFilters: RecipeListFilterValues = {
  search: "",
  category: "",
  tags: [],
  tagId: "",
  tagName: "",
  favoritesOnly: false,
  sort: "alphabetical",
  ownership: "all",
  showTagsInMain: false,
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
    setStoredState((current) => {
      const normalized = normalizeState(current)
      return {
        ...normalized,
        filters: {
          ...emptyFilters,
          showTagsInMain: normalized.filters.showTagsInMain,
        },
      }
    })
  }, [setStoredState])

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
    setSort: (sort: RecipeListSort) => {
      updateFilters({ sort })
    },
    setOwnership: (ownership: RecipeOwnershipFilter) => {
      updateFilters({ ownership })
    },
    setShowTagsInMain: (showTagsInMain: boolean) => {
      updateFilters({ showTagsInMain })
    },
    resetActiveFilters: resetFilters,
  }
}

function normalizeState(state: StoredRecipeListFilterState): RecipeListFilterState {
  if (isLegacyRecipeListFilterState(state)) {
    return {
      scope: state.scope,
      filters: normalizeFilters(state.filters[state.scope]),
    }
  }

  return {
    scope: state.scope,
    filters: normalizeFilters(state.filters),
  }
}

function normalizeFilters(filters: Partial<RecipeListFilterValues>): RecipeListFilterValues {
  return {
    ...emptyFilters,
    ...filters,
    tags: filters.tags ?? [],
    sort: isRecipeListSort(filters.sort) ? filters.sort : emptyFilters.sort,
    ownership: isRecipeOwnershipFilter(filters.ownership)
      ? filters.ownership
      : emptyFilters.ownership,
    showTagsInMain: filters.showTagsInMain ?? emptyFilters.showTagsInMain,
  }
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

function isFilterValues(value: unknown): value is Partial<RecipeListFilterValues> {
  return (
    isRecord(value) &&
    typeof value.search === "string" &&
    typeof value.category === "string" &&
    (value.tags === undefined ||
      (Array.isArray(value.tags) && value.tags.every(isRecipeListTagFilter))) &&
    (value.tagId === undefined || typeof value.tagId === "string") &&
    (value.tagName === undefined || typeof value.tagName === "string") &&
    typeof value.favoritesOnly === "boolean" &&
    (value.sort === undefined || isRecipeListSort(value.sort)) &&
    (value.ownership === undefined || isRecipeOwnershipFilter(value.ownership)) &&
    (value.showTagsInMain === undefined || typeof value.showTagsInMain === "boolean")
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

function isRecipeListSort(value: unknown): value is RecipeListSort {
  return value === "alphabetical" || value === "newest" || value === "popular"
}

function isRecipeOwnershipFilter(value: unknown): value is RecipeOwnershipFilter {
  return value === "all" || value === "mine" || value === "others"
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null
}
