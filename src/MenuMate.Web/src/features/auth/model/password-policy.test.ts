import { describe, expect, it } from "vitest"

import { passwordPairSchema, passwordSchema } from "./password-policy"

describe("password policy", () => {
  it.each([
    [7, false],
    [8, true],
    [128, true],
    [129, false],
  ])("validates %i characters", (length, expected) => {
    expect(passwordSchema.safeParse("a".repeat(length)).success).toBe(expected)
  })

  it("requires matching confirmation", () => {
    expect(
      passwordPairSchema.safeParse({ password: "password", confirmPassword: "different" }).success,
    ).toBe(false)
  })
})
