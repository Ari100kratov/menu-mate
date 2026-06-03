import { create } from "zustand"
import { persist } from "zustand/middleware"

import {
  defaultShoppingCategory,
  defaultShoppingUnit,
  type ShoppingCategoryValue,
  type ShoppingUnitValue,
} from "@/shared/config/shopping-taxonomy"

interface UserPreferencesState {
  defaultShoppingUnit: ShoppingUnitValue
  defaultShoppingCategory: ShoppingCategoryValue
  setDefaultShoppingUnit: (value: ShoppingUnitValue) => void
  setDefaultShoppingCategory: (value: ShoppingCategoryValue) => void
  resetShoppingDefaults: () => void
}

export const useUserPreferencesStore = create<UserPreferencesState>()(
  persist(
    (set) => ({
      defaultShoppingUnit,
      defaultShoppingCategory,
      setDefaultShoppingUnit: (value) => {
        set({ defaultShoppingUnit: value })
      },
      setDefaultShoppingCategory: (value) => {
        set({ defaultShoppingCategory: value })
      },
      resetShoppingDefaults: () => {
        set({
          defaultShoppingUnit,
          defaultShoppingCategory,
        })
      },
    }),
    {
      name: "menumate:user-preferences:v1",
      version: 1,
    },
  ),
)
