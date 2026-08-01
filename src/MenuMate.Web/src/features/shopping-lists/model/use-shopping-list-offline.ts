import { useQueryClient } from "@tanstack/react-query"
import { useCallback, useEffect, useMemo, useRef, useState } from "react"
import { toast } from "sonner"

import { useOfflineShoppingRecordQuery } from "@/features/shopping-lists/api/shopping-list-offline.queries"
import { offlineShoppingRecordQueryKey } from "@/features/shopping-lists/api/shopping-list-offline.queries"
import {
  getShoppingList,
  setShoppingListItemState,
  type ShoppingList,
} from "@/features/shopping-lists/api/shopping-lists.api"
import {
  shoppingListQueryKeys,
  useShoppingListQuery,
} from "@/features/shopping-lists/api/shopping-lists.queries"
import {
  discardOfflineShoppingItemState,
  getEffectiveOfflineShoppingList,
  mergeOfflineShoppingRecord,
  queueOfflineShoppingItemState,
  settleOfflineShoppingItemState,
  type OfflineShoppingRecord,
} from "@/features/shopping-lists/model/shopping-list-offline"
import { writeOfflineShoppingRecord } from "@/features/shopping-lists/model/shopping-list-offline.storage"
import { findShoppingListItem } from "@/features/shopping-lists/model/shopping-list-text"
import { ApiException } from "@/shared/api/errors"
import { useSessionStore } from "@/shared/auth/session.store"
import { useNetworkStatus } from "@/shared/lib/network-status"

type ShoppingSyncStatus = "idle" | "syncing" | "waiting"

export interface ShoppingOfflineStatus {
  mode: "offline" | "syncing" | "waiting"
  pendingCount: number
  savedAt: string | null
}

export function useShoppingListOffline() {
  const queryClient = useQueryClient()
  const user = useSessionStore((state) => state.user)
  const accessToken = useSessionStore((state) => state.accessToken)
  const offlineAccess = useSessionStore((state) => state.offlineAccess)
  const isBrowserOnline = useNetworkStatus()
  const offlineRecordQuery = useOfflineShoppingRecordQuery()
  const shoppingListQuery = useShoppingListQuery(Boolean(accessToken && user))
  const synchronizationRef = useRef(false)
  const mountedRef = useRef(true)
  const [syncStatus, setSyncStatus] = useState<ShoppingSyncStatus>("idle")
  const [retryVersion, setRetryVersion] = useState(0)
  const [showSynchronizationStatus, setShowSynchronizationStatus] = useState(false)

  useEffect(() => {
    mountedRef.current = true
    return () => {
      mountedRef.current = false
    }
  }, [])

  const record = useMemo(() => {
    const cachedUserId = offlineRecordQuery.data?.user.id
    return cachedUserId !== undefined && cachedUserId === user?.id
      ? (offlineRecordQuery.data ?? null)
      : null
  }, [offlineRecordQuery.data, user])
  const pendingSignature = record
    ? Object.entries(record.pendingItemStates)
        .sort(([first], [second]) => first.localeCompare(second))
        .map(([itemId, isPurchased]) => `${itemId}:${String(isPurchased)}`)
        .join("|")
    : ""
  const pendingCount = record ? Object.keys(record.pendingItemStates).length : 0
  const hasServerList = Boolean(shoppingListQuery.data)

  const commitRecord = useCallback(
    (nextRecord: OfflineShoppingRecord) => {
      queryClient.setQueryData(offlineShoppingRecordQueryKey, nextRecord)
      void writeOfflineShoppingRecord(nextRecord).catch(() => {
        toast.error("Не удалось сохранить изменения для офлайн-режима")
      })
    },
    [queryClient],
  )

  useEffect(() => {
    if (!shoppingListQuery.data || !user || offlineRecordQuery.isPending) {
      return
    }

    const current = queryClient.getQueryData<OfflineShoppingRecord | null>(
      offlineShoppingRecordQueryKey,
    )
    commitRecord(mergeOfflineShoppingRecord(current, user, shoppingListQuery.data))
  }, [commitRecord, offlineRecordQuery.isPending, queryClient, shoppingListQuery.data, user])

  useEffect(() => {
    if (
      !accessToken ||
      !user ||
      !isBrowserOnline ||
      !hasServerList ||
      pendingCount === 0 ||
      synchronizationRef.current
    ) {
      return
    }

    const activeUser = user
    synchronizationRef.current = true

    async function synchronize() {
      setSyncStatus("syncing")
      const initialRecord = queryClient.getQueryData<OfflineShoppingRecord>(
        offlineShoppingRecordQueryKey,
      )
      const initialShoppingList = queryClient.getQueryData<ShoppingList>(
        shoppingListQueryKeys.current,
      )
      if (!initialShoppingList || initialRecord?.user.id !== activeUser.id) {
        synchronizationRef.current = false
        setSyncStatus("idle")
        return
      }

      let synchronizedList = initialShoppingList
      let skippedItemCount = 0
      let completed = false

      try {
        const pendingSnapshot = Object.entries(initialRecord.pendingItemStates)

        for (const [itemId, isPurchased] of pendingSnapshot) {
          if (!mountedRef.current) {
            return
          }

          const currentRecord = queryClient.getQueryData<OfflineShoppingRecord>(
            offlineShoppingRecordQueryKey,
          )
          if (currentRecord?.user.id !== activeUser.id) {
            return
          }

          if (!findShoppingListItem(synchronizedList, itemId)) {
            skippedItemCount += 1
            commitRecord(discardOfflineShoppingItemState(currentRecord, itemId))
            continue
          }

          try {
            synchronizedList = await setShoppingListItemState(itemId, { isPurchased })
          } catch (error) {
            if (error instanceof ApiException && error.status === 404) {
              skippedItemCount += 1
              commitRecord(discardOfflineShoppingItemState(currentRecord, itemId))
              continue
            }

            throw error
          }

          queryClient.setQueryData(shoppingListQueryKeys.current, synchronizedList)
          const latestRecord =
            queryClient.getQueryData<OfflineShoppingRecord>(offlineShoppingRecordQueryKey) ??
            currentRecord
          commitRecord(
            settleOfflineShoppingItemState(latestRecord, itemId, isPurchased, synchronizedList),
          )
        }

        if (!mountedRef.current) {
          return
        }

        const freshList = await getShoppingList()
        queryClient.setQueryData(shoppingListQueryKeys.current, freshList)
        const latestRecord = queryClient.getQueryData<OfflineShoppingRecord>(
          offlineShoppingRecordQueryKey,
        )
        if (latestRecord?.user.id === activeUser.id) {
          commitRecord(mergeOfflineShoppingRecord(latestRecord, activeUser, freshList))
        }

        setSyncStatus("idle")
        if (skippedItemCount > 0) {
          toast.warning(
            skippedItemCount === 1
              ? "Одна офлайн-отметка пропущена: список изменился на сервере"
              : `${String(skippedItemCount)} офлайн-отметки пропущены: список изменился на сервере`,
          )
        }
        completed = true
      } catch {
        if (mountedRef.current) {
          setSyncStatus("waiting")
        }
      } finally {
        synchronizationRef.current = false
        const latestRecord = queryClient.getQueryData<OfflineShoppingRecord>(
          offlineShoppingRecordQueryKey,
        )
        const hasPendingStates = Object.keys(latestRecord?.pendingItemStates ?? {}).length > 0
        if (completed && mountedRef.current && hasPendingStates) {
          setRetryVersion((current) => current + 1)
        }
      }
    }

    void synchronize()
  }, [
    accessToken,
    commitRecord,
    hasServerList,
    isBrowserOnline,
    pendingCount,
    pendingSignature,
    queryClient,
    retryVersion,
    user,
  ])

  useEffect(() => {
    if (syncStatus !== "waiting" || !isBrowserOnline || pendingCount === 0) {
      return
    }

    const timeout = window.setTimeout(() => {
      setRetryVersion((current) => current + 1)
    }, 15_000)

    return () => {
      window.clearTimeout(timeout)
    }
  }, [isBrowserOnline, pendingCount, syncStatus])

  const isSynchronizationActive =
    !offlineAccess &&
    isBrowserOnline &&
    syncStatus !== "waiting" &&
    (syncStatus === "syncing" || pendingCount > 0)

  useEffect(() => {
    const timeout = window.setTimeout(
      () => {
        setShowSynchronizationStatus(isSynchronizationActive)
      },
      isSynchronizationActive ? 500 : 0,
    )

    return () => {
      window.clearTimeout(timeout)
    }
  }, [isSynchronizationActive])

  useEffect(() => {
    function retryOnFocus() {
      if (document.visibilityState === "visible") {
        setRetryVersion((current) => current + 1)
      }
    }

    document.addEventListener("visibilitychange", retryOnFocus)
    return () => {
      document.removeEventListener("visibilitychange", retryOnFocus)
    }
  }, [])

  const setItemPurchasedState = useCallback(
    (itemId: string, isPurchased: boolean) => {
      const currentRecord = queryClient.getQueryData<OfflineShoppingRecord>(
        offlineShoppingRecordQueryKey,
      )
      if (!user || currentRecord?.user.id !== user.id) {
        return
      }

      commitRecord(queueOfflineShoppingItemState(currentRecord, itemId, isPurchased))
      setRetryVersion((current) => current + 1)
    },
    [commitRecord, queryClient, user],
  )

  const hasCachedList = record !== null
  const isConnectionUnavailable =
    offlineAccess ||
    !isBrowserOnline ||
    shoppingListQuery.fetchStatus === "paused" ||
    Boolean(shoppingListQuery.error && hasCachedList)
  const isReadOnly = isConnectionUnavailable || syncStatus === "syncing" || pendingCount > 0
  const shoppingList = record ? getEffectiveOfflineShoppingList(record) : shoppingListQuery.data
  const error = hasCachedList ? null : (shoppingListQuery.error ?? offlineRecordQuery.error)

  let offlineStatus: ShoppingOfflineStatus | null = null
  if (isConnectionUnavailable) {
    offlineStatus = {
      mode: "offline",
      pendingCount,
      savedAt: record?.savedAt ?? null,
    }
  } else if (syncStatus === "waiting") {
    offlineStatus = {
      mode: "waiting",
      pendingCount,
      savedAt: record?.savedAt ?? null,
    }
  } else if (showSynchronizationStatus) {
    offlineStatus = {
      mode: "syncing",
      pendingCount,
      savedAt: record?.savedAt ?? null,
    }
  }

  return {
    shoppingList,
    error,
    isPending: !shoppingList && (shoppingListQuery.isPending || offlineRecordQuery.isPending),
    isOffline: isConnectionUnavailable,
    isReadOnly,
    offlineStatus,
    setItemPurchasedState,
  }
}
