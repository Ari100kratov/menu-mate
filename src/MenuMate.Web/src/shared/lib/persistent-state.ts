import { useEffect, useState, type Dispatch, type SetStateAction } from "react"

export function usePersistentState<T>(
  storageKey: string,
  initialValue: T,
  isValidValue: (value: unknown) => value is T,
): [T, Dispatch<SetStateAction<T>>] {
  const [value, setValue] = useState<T>(() => {
    try {
      const storedValue = window.localStorage.getItem(storageKey)
      if (!storedValue) {
        return initialValue
      }

      const parsedValue: unknown = JSON.parse(storedValue)
      return isValidValue(parsedValue) ? parsedValue : initialValue
    } catch {
      return initialValue
    }
  })

  useEffect(() => {
    try {
      window.localStorage.setItem(storageKey, JSON.stringify(value))
    } catch {
      // Навигация продолжает работать, даже если хранилище недоступно или переполнено.
    }
  }, [storageKey, value])

  return [value, setValue]
}

export function readPersistentString(storageKey: string) {
  try {
    return window.localStorage.getItem(storageKey)
  } catch {
    return null
  }
}

export function writePersistentString(storageKey: string, value: string) {
  try {
    window.localStorage.setItem(storageKey, value)
  } catch {
    // Отсутствие localStorage не должно блокировать переход между разделами.
  }
}
