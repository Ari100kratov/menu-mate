import { useSyncExternalStore } from "react"

const networkStatusEventNames = ["online", "offline"] as const

export function useNetworkStatus() {
  return useSyncExternalStore(subscribe, getSnapshot, getServerSnapshot)
}

function subscribe(onStoreChange: () => void) {
  for (const eventName of networkStatusEventNames) {
    window.addEventListener(eventName, onStoreChange)
  }

  return () => {
    for (const eventName of networkStatusEventNames) {
      window.removeEventListener(eventName, onStoreChange)
    }
  }
}

function getSnapshot() {
  return window.navigator.onLine
}

function getServerSnapshot() {
  return true
}
