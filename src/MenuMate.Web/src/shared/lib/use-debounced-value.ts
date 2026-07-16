import { useEffect, useState } from "react"

export function useDebouncedValue<T>(value: T, delayMilliseconds: number) {
  const [debouncedValue, setDebouncedValue] = useState(value)

  useEffect(() => {
    const timer = window.setTimeout(() => {
      setDebouncedValue(value)
    }, delayMilliseconds)

    return () => {
      window.clearTimeout(timer)
    }
  }, [delayMilliseconds, value])

  return debouncedValue
}
