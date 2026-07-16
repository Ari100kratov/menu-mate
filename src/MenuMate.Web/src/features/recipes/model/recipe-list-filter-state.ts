import { useCallback } from "react"

import { usePersistentState } from "@/shared/lib/persistent-state"

export type RecipeListScope = "library" | "catalog"

export interface RecipeListFilterValues {
  search: string
  category: string
  favoritesOnly: boolean
}

interface RecipeListFilterState {
  scope: RecipeListScope
  filters: Record<RecipeListScope, RecipeListFilterValues>
}

const emptyFilters: RecipeListFilterValues = {
  search: "",
  category: "",
  favoritesOnly: false,
}

const initialState: RecipeListFilterState = {
  scope: "library",
  filters: {
    library: { ...emptyFilters },
    catalog: { ...emptyFilters },
  },
}

export function useRecipeListFilterState(storageKey: string) {
  const [state, setState] = usePersistentState(storageKey, initialState, isRecipeListFilterState)
  const activeFilters = state.filters[state.scope]

  const setScope = useCallback(
    (scope: RecipeListScope) => {
      setState((current) => ({ ...current, scope }))
    },
    [setState],
  )

  const updateActiveFilters = useCallback(
    (updates: Partial<RecipeListFilterValues>) => {
      setState((current) => ({
        ...current,
        filters: {
          ...current.filters,
          [current.scope]: {
            ...current.filters[current.scope],
            ...updates,
          },
        },
      }))
    },
    [setState],
  )

  const resetActiveFilters = useCallback(() => {
    updateActiveFilters(emptyFilters)
  }, [updateActiveFilters])

  return {
    scope: state.scope,
    ...activeFilters,
    setScope,
    setSearch: (search: string) => {
      updateActiveFilters({ search })
    },
    setCategory: (category: string) => {
      updateActiveFilters({ category })
    },
    setFavoritesOnly: (favoritesOnly: boolean) => {
      updateActiveFilters({ favoritesOnly })
    },
    resetActiveFilters,
  }
}

function isRecipeListFilterState(value: unknown): value is RecipeListFilterState {
  if (!isRecord(value) || (value.scope !== "library" && value.scope !== "catalog")) {
    return false
  }

  if (!isRecord(value.filters)) {
    return false
  }

  return isFilterValues(value.filters.library) && isFilterValues(value.filters.catalog)
}

function isFilterValues(value: unknown): value is RecipeListFilterValues {
  return (
    isRecord(value) &&
    typeof value.search === "string" &&
    typeof value.category === "string" &&
    typeof value.favoritesOnly === "boolean"
  )
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null
}
