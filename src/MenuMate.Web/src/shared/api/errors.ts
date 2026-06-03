export interface ApiProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  traceId?: string
  code?: string
  errors?: Record<string, string[]> | ApiErrorItem[]
}

export interface ApiErrorItem {
  code?: string
  description?: string
  type?: number
}

export class ApiException extends Error {
  public readonly status?: number
  public readonly title?: string
  public readonly detail?: string
  public readonly traceId?: string
  public readonly errors?: ApiProblemDetails["errors"]

  public constructor(problem: ApiProblemDetails) {
    super(problem.detail ?? problem.title ?? "Неизвестная ошибка API")
    this.name = "ApiException"
    this.status = problem.status
    this.title = problem.title
    this.detail = problem.detail
    this.traceId = problem.traceId
    this.errors = problem.errors
  }
}

export function toApiException(error: unknown, status?: number) {
  if (isProblemDetails(error)) {
    return new ApiException({ ...error, status: error.status ?? status })
  }

  if (error instanceof Error) {
    return error
  }

  return new Error(formatUnknownError(error, status))
}

export function getApiErrorMessages(error: unknown): string[] {
  if (error instanceof ApiException) {
    if (Array.isArray(error.errors)) {
      return error.errors
        .map((item) => item.description)
        .filter((description): description is string => Boolean(description))
    }

    if (error.errors) {
      return Object.values(error.errors).flat()
    }

    return [error.detail ?? error.title ?? error.message]
  }

  if (error instanceof Error) {
    return [error.message]
  }

  return [formatUnknownError(error)]
}

function isProblemDetails(value: unknown): value is ApiProblemDetails {
  return (
    isRecord(value) &&
    (typeof value.title === "string" || typeof value.detail === "string") &&
    (value.status === undefined || typeof value.status === "number")
  )
}

function formatUnknownError(error: unknown, status?: number) {
  if (typeof error === "string") {
    return error
  }

  if (status) {
    return `Ошибка ${String(status)}`
  }

  return "Неизвестная ошибка"
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null
}
