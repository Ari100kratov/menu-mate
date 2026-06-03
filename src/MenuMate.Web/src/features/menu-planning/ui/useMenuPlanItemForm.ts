import { useForm } from "@tanstack/react-form"

import type { MenuPlan } from "@/features/menu-planning/api/menu-plans.api"
import {
  createMenuPlanItemFormSchema,
  type MenuPlanItemFormValues,
} from "@/features/menu-planning/model/menu-plan-form"

interface UseMenuPlanItemFormOptions {
  menuPlan: MenuPlan
  initialValues: MenuPlanItemFormValues
  resetAfterSubmit: boolean
  onSubmit: (values: MenuPlanItemFormValues) => void
}

export function useMenuPlanItemForm({
  menuPlan,
  initialValues,
  resetAfterSubmit,
  onSubmit,
}: UseMenuPlanItemFormOptions) {
  const form = useForm({
    defaultValues: initialValues,
    validators: {
      onSubmit: createMenuPlanItemFormSchema(menuPlan),
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

export type MenuPlanItemFormApi = ReturnType<typeof useMenuPlanItemForm>
