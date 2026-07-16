export interface BackNavigationState {
  backTo: string
  backState?: unknown
}

interface NavigationLocation {
  pathname: string
  search: string
  hash: string
  state: unknown
}

export function createBackNavigationState(location: NavigationLocation): BackNavigationState {
  const previousState = getBackNavigationState(location.state)

  return {
    backTo: `${location.pathname}${location.search}${location.hash}`,
    ...(previousState ? { backState: previousState } : {}),
  }
}

export function getBackNavigationState(value: unknown): BackNavigationState | undefined {
  if (!isRecord(value) || !isInternalPath(value.backTo)) {
    return undefined
  }

  return {
    backTo: value.backTo,
    ...(value.backState === undefined ? {} : { backState: value.backState }),
  }
}

export function getParentBackState(value: unknown) {
  return getBackNavigationState(value)?.backState
}

function isInternalPath(value: unknown): value is string {
  return typeof value === "string" && value.startsWith("/") && !value.startsWith("//")
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null
}
