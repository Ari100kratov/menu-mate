import { z } from "zod"

export const passwordSchema = z
  .string()
  .min(8, "Пароль должен быть не короче 8 символов.")
  .max(128, "Пароль должен быть не длиннее 128 символов.")

export const passwordPairSchema = z
  .object({
    password: passwordSchema,
    confirmPassword: z.string(),
  })
  .refine((value) => value.password === value.confirmPassword, {
    path: ["confirmPassword"],
    message: "Пароли не совпадают.",
  })
