import { useForm } from "@tanstack/react-form"

import {
  shoppingItemFormSchema,
  type ShoppingItemFormValues,
} from "@/features/shopping-lists/model/shopping-list-ui"

interface UseShoppingItemFormOptions {
  initialValues: ShoppingItemFormValues
  resetAfterSubmit: boolean
  onSubmit: (values: ShoppingItemFormValues) => void
}

export function useShoppingItemForm({
  initialValues,
  resetAfterSubmit,
  onSubmit,
}: UseShoppingItemFormOptions) {
  const form = useForm({
    defaultValues: initialValues,
    validators: {
      onSubmit: shoppingItemFormSchema,
    },
    onSubmit: ({ value }) => {
      onSubmit(value)

      if (resetAfterSubmit) {
        form.reset()
      }
    },
  })

  return form
}

export type ShoppingItemFormApi = ReturnType<typeof useShoppingItemForm>
