import type { RecipeFormApi } from "@/features/recipes/ui/useRecipeForm"
import { Field, FieldError, FieldLabel } from "@/shared/ui/field"
import { Input } from "@/shared/ui/input"

interface RecipeDurationFieldProps {
  form: RecipeFormApi
  name: "totalTimeMinutes" | "activeTimeMinutes"
  label: string
  hint: string
}

export function RecipeDurationField({ form, name, label, hint }: RecipeDurationFieldProps) {
  return (
    <form.Field name={name}>
      {(field) => {
        const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid
        const totalMinutes = Number(field.state.value || 0)
        const hours = field.state.value ? String(Math.floor(totalMinutes / 60)) : ""
        const minutes = field.state.value ? String(totalMinutes % 60) : ""

        function updateDuration(nextHours: string, nextMinutes: string) {
          const normalizedHours = clampNumber(nextHours, 168)
          const normalizedMinutes = clampNumber(nextMinutes, 59)
          const nextTotal = normalizedHours * 60 + normalizedMinutes
          field.handleChange(nextTotal > 0 ? String(nextTotal) : "")
        }

        return (
          <Field data-invalid={isInvalid}>
            <div className="flex items-baseline justify-between gap-3">
              <FieldLabel>{label}</FieldLabel>
              <span className="type-supporting text-muted-foreground">{hint}</span>
            </div>
            <div className="grid grid-cols-2 gap-2">
              <label
                className="bg-muted/50 aria-invalid:border-destructive aria-invalid:ring-destructive/20 flex items-center rounded-lg border px-3 aria-invalid:ring-[3px]"
                aria-invalid={isInvalid}
              >
                <Input
                  className="h-11 border-0 bg-transparent px-0 text-center text-base font-medium shadow-none focus-visible:ring-0"
                  inputMode="numeric"
                  value={hours}
                  placeholder="0"
                  aria-label={`${label}, часы`}
                  onBlur={field.handleBlur}
                  onChange={(event) => {
                    updateDuration(event.target.value, minutes)
                  }}
                />
                <span className="type-supporting text-muted-foreground">ч</span>
              </label>
              <label
                className="bg-muted/50 aria-invalid:border-destructive aria-invalid:ring-destructive/20 flex items-center rounded-lg border px-3 aria-invalid:ring-[3px]"
                aria-invalid={isInvalid}
              >
                <Input
                  className="h-11 border-0 bg-transparent px-0 text-center text-base font-medium shadow-none focus-visible:ring-0"
                  inputMode="numeric"
                  value={minutes}
                  placeholder="0"
                  aria-label={`${label}, минуты`}
                  onBlur={field.handleBlur}
                  onChange={(event) => {
                    updateDuration(hours, event.target.value)
                  }}
                />
                <span className="type-supporting text-muted-foreground">мин</span>
              </label>
            </div>
            {isInvalid ? <FieldError errors={field.state.meta.errors} /> : null}
          </Field>
        )
      }}
    </form.Field>
  )
}

function clampNumber(value: string, max: number) {
  const digits = value.replace(/\D/g, "")
  return digits ? Math.min(Number(digits), max) : 0
}
