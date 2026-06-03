import { toApiException } from "@/shared/api/errors"

interface ApiResponse<TData> {
  data?: TData
  error?: unknown
  response: Response
}

interface EmptyApiResponse {
  data?: unknown
  error?: unknown
  response: Response
}

export async function unwrapApiResponse<TData>(
  responsePromise: Promise<ApiResponse<TData>>,
): Promise<TData> {
  const response = await responsePromise
  ensureApiSuccess(response)

  if (response.data === undefined) {
    throw new Error("API вернул пустой ответ вместо ожидаемых данных.")
  }

  return response.data
}

export async function unwrapEmptyApiResponse(
  responsePromise: Promise<EmptyApiResponse>,
): Promise<void> {
  const response = await responsePromise
  ensureApiSuccess(response)
}

function ensureApiSuccess(response: EmptyApiResponse) {
  if (response.error) {
    throw toApiException(response.error, response.response.status)
  }

  if (!response.response.ok) {
    throw toApiException(response.response.statusText, response.response.status)
  }
}
