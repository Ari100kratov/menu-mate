import type { UserProfile } from "@/features/auth/api/auth.api"
import type { ShoppingList } from "@/features/shopping-lists/api/shopping-lists.api"
import {
  findShoppingListItem,
  refreshShoppingListText,
  setShoppingListItemPurchasedState,
} from "@/features/shopping-lists/model/shopping-list-text"

export const offlineShoppingRecordVersion = 1

export interface OfflineShoppingRecord {
  version: typeof offlineShoppingRecordVersion
  user: UserProfile
  shoppingList: ShoppingList
  pendingItemStates: Record<string, boolean>
  savedAt: string
}

export function createOfflineShoppingRecord(
  user: UserProfile,
  shoppingList: ShoppingList,
): OfflineShoppingRecord {
  return {
    version: offlineShoppingRecordVersion,
    user,
    shoppingList,
    pendingItemStates: {},
    savedAt: new Date().toISOString(),
  }
}

export function mergeOfflineShoppingRecord(
  current: OfflineShoppingRecord | null | undefined,
  user: UserProfile,
  shoppingList: ShoppingList,
): OfflineShoppingRecord {
  if (current?.user.id !== user.id) {
    return createOfflineShoppingRecord(user, shoppingList)
  }

  return {
    ...current,
    user,
    shoppingList,
    savedAt: new Date().toISOString(),
  }
}

export function getEffectiveOfflineShoppingList(record: OfflineShoppingRecord) {
  return refreshShoppingListText(
    Object.entries(record.pendingItemStates).reduce(
      (shoppingList, [itemId, isPurchased]) =>
        setShoppingListItemPurchasedState(shoppingList, itemId, isPurchased),
      record.shoppingList,
    ),
  )
}

export function queueOfflineShoppingItemState(
  record: OfflineShoppingRecord,
  itemId: string,
  isPurchased: boolean,
): OfflineShoppingRecord {
  const serverItem = findShoppingListItem(record.shoppingList, itemId)
  if (!serverItem) {
    return record
  }

  let pendingItemStates = record.pendingItemStates
  if (serverItem.isPurchased === isPurchased) {
    pendingItemStates = omitPendingItemState(pendingItemStates, itemId)
  } else {
    pendingItemStates = {
      ...pendingItemStates,
      [itemId]: isPurchased,
    }
  }

  return {
    ...record,
    pendingItemStates,
    savedAt: new Date().toISOString(),
  }
}

export function settleOfflineShoppingItemState(
  record: OfflineShoppingRecord,
  itemId: string,
  appliedState: boolean,
  shoppingList: ShoppingList,
): OfflineShoppingRecord {
  const pendingItemStates =
    record.pendingItemStates[itemId] === appliedState
      ? omitPendingItemState(record.pendingItemStates, itemId)
      : record.pendingItemStates

  return {
    ...record,
    shoppingList,
    pendingItemStates,
    savedAt: new Date().toISOString(),
  }
}

export function discardOfflineShoppingItemState(
  record: OfflineShoppingRecord,
  itemId: string,
): OfflineShoppingRecord {
  return {
    ...record,
    pendingItemStates: omitPendingItemState(record.pendingItemStates, itemId),
    savedAt: new Date().toISOString(),
  }
}

export function isOfflineShoppingRecord(value: unknown): value is OfflineShoppingRecord {
  if (!isRecord(value) || value.version !== offlineShoppingRecordVersion) {
    return false
  }

  return (
    isUserProfile(value.user) &&
    isShoppingList(value.shoppingList) &&
    isBooleanRecord(value.pendingItemStates) &&
    typeof value.savedAt === "string"
  )
}

function isUserProfile(value: unknown): value is UserProfile {
  return (
    isRecord(value) &&
    typeof value.id === "string" &&
    typeof value.email === "string" &&
    typeof value.displayName === "string" &&
    Array.isArray(value.roles) &&
    value.roles.every((role) => typeof role === "string") &&
    isRecord(value.preferences) &&
    typeof value.preferences.showShoppingListPreview === "boolean"
  )
}

function isShoppingList(value: unknown): value is ShoppingList {
  return (
    isRecord(value) &&
    typeof value.id === "string" &&
    typeof value.createdAt === "string" &&
    typeof value.updatedAt === "string" &&
    Array.isArray(value.categories) &&
    typeof value.text === "string" &&
    typeof value.remainingText === "string"
  )
}

function isBooleanRecord(value: unknown): value is Record<string, boolean> {
  return isRecord(value) && Object.values(value).every((item) => typeof item === "boolean")
}

function omitPendingItemState(pendingItemStates: Record<string, boolean>, itemId: string) {
  return Object.fromEntries(
    Object.entries(pendingItemStates).filter(([pendingItemId]) => pendingItemId !== itemId),
  )
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null
}
