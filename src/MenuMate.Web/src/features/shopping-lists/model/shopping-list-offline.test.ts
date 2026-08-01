import "fake-indexeddb/auto"
import { beforeEach, describe, expect, it } from "vitest"

import type { UserProfile } from "@/features/auth/api/auth.api"
import type { ShoppingList } from "@/features/shopping-lists/api/shopping-lists.api"
import {
  createOfflineShoppingRecord,
  getEffectiveOfflineShoppingList,
  queueOfflineShoppingItemState,
  settleOfflineShoppingItemState,
} from "@/features/shopping-lists/model/shopping-list-offline"
import {
  clearOfflineShoppingRecord,
  readOfflineShoppingRecord,
  writeOfflineShoppingRecord,
} from "@/features/shopping-lists/model/shopping-list-offline.storage"
import { setShoppingListItemPurchasedState } from "@/features/shopping-lists/model/shopping-list-text"

const user: UserProfile = {
  id: "01900000-0000-7000-8000-000000000001",
  email: "shopper@example.com",
  displayName: "Покупатель",
  roles: ["user"],
  preferences: {
    showShoppingListPreview: true,
  },
}

const shoppingList: ShoppingList = {
  id: "01900000-0000-7000-8000-000000000010",
  sourceStartDate: null,
  sourceEndDate: null,
  createdAt: "2026-08-01T10:00:00Z",
  updatedAt: "2026-08-01T10:00:00Z",
  categories: [
    {
      name: "Молочные продукты",
      items: [
        {
          id: "01900000-0000-7000-8000-000000000011",
          productId: "01900000-0000-7000-8000-000000000012",
          name: "Молоко",
          amount: 2,
          unit: "Liter",
          category: "Dairy",
          amountText: "2 л",
          comment: "для кофе",
          isPurchased: false,
        },
      ],
    },
  ],
  text: "Молочные продукты\n- Молоко 2 л (для кофе)",
  remainingText: "Молочные продукты\n- Молоко 2 л (для кофе)",
}

describe("offline shopping state", () => {
  it("refreshes share text saved by an older frontend version", () => {
    const record = createOfflineShoppingRecord(user, {
      ...shoppingList,
      text: "Молочные продукты\n- [ ] Молоко 2 л (для кофе)",
      remainingText: "Молочные продукты\n- [ ] Молоко 2 л (для кофе)",
    })

    const effectiveList = getEffectiveOfflineShoppingList(record)

    expect(effectiveList.text).toContain("- Молоко 2 л (для кофе)")
    expect(effectiveList.text).not.toContain("[ ]")
    expect(effectiveList.remainingText).not.toContain("[ ]")
  })

  it("keeps only the final state and refreshes share text", () => {
    const initial = createOfflineShoppingRecord(user, shoppingList)
    const purchased = queueOfflineShoppingItemState(
      initial,
      shoppingList.categories[0]?.items[0]?.id ?? "",
      true,
    )

    expect(purchased.pendingItemStates).toEqual({
      "01900000-0000-7000-8000-000000000011": true,
    })
    expect(getEffectiveOfflineShoppingList(purchased).text).toContain("- ✓ Молоко")
    expect(getEffectiveOfflineShoppingList(purchased).remainingText).toBe("")

    const restored = queueOfflineShoppingItemState(
      purchased,
      "01900000-0000-7000-8000-000000000011",
      false,
    )
    expect(restored.pendingItemStates).toEqual({})
  })

  it("does not discard a newer local state after an older request completes", () => {
    const initial = createOfflineShoppingRecord(user, shoppingList)
    const queued = queueOfflineShoppingItemState(
      initial,
      "01900000-0000-7000-8000-000000000011",
      true,
    )
    const changedAgain = {
      ...queued,
      pendingItemStates: {
        "01900000-0000-7000-8000-000000000011": false,
      },
    }
    const serverList = setShoppingListItemPurchasedState(
      shoppingList,
      "01900000-0000-7000-8000-000000000011",
      true,
    )

    const settled = settleOfflineShoppingItemState(
      changedAgain,
      "01900000-0000-7000-8000-000000000011",
      true,
      serverList,
    )

    expect(settled.pendingItemStates).toEqual({
      "01900000-0000-7000-8000-000000000011": false,
    })
  })
})

describe("offline shopping storage", () => {
  beforeEach(async () => {
    await clearOfflineShoppingRecord()
  })

  it("restores the list and pending states after a reload", async () => {
    const record = queueOfflineShoppingItemState(
      createOfflineShoppingRecord(user, shoppingList),
      "01900000-0000-7000-8000-000000000011",
      true,
    )

    await writeOfflineShoppingRecord(record)

    await expect(readOfflineShoppingRecord()).resolves.toEqual(record)
  })
})
