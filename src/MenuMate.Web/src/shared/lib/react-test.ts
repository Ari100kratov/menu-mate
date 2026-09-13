import { act } from "react"

export async function flushReact(action: () => void | Promise<void>) {
  await act(async () => {
    await action()
  })
}
