import { CircleCheck, Search, Sparkles } from "lucide-react"

import { useProductsQuery } from "@/features/products/api/products.queries"
import type {
  RecipeIngredientFieldErrors,
  RecipeIngredientFormValues,
} from "@/features/recipes/model/recipe-form"
import {
  getProductCategoryLabel,
  measurementUnitOptions,
  productCategoryOptions,
  quantityKindOptions,
} from "@/features/recipes/model/recipe-form-options"
import { Checkbox } from "@/shared/ui/checkbox"
import { Field, FieldDescription, FieldError, FieldLabel } from "@/shared/ui/field"
import { Input } from "@/shared/ui/input"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/shared/ui/select"
import { Textarea } from "@/shared/ui/textarea"

interface RecipeIngredientEditorFieldsProps {
  value: RecipeIngredientFormValues
  errors: RecipeIngredientFieldErrors
  onChange: (value: RecipeIngredientFormValues) => void
}

export function RecipeIngredientEditorFields({
  value,
  errors,
  onChange,
}: RecipeIngredientEditorFieldsProps) {
  const productsQuery = useProductsQuery(value.productName)
  const suggestions =
    value.productName.trim() && !value.ingredientId ? (productsQuery.data ?? []).slice(0, 6) : []

  function update(patch: Partial<RecipeIngredientFormValues>) {
    onChange({ ...value, ...patch })
  }

  return (
    <div className="space-y-6">
      <Field data-editor-invalid={Boolean(errors.productName)}>
        <FieldLabel htmlFor="ingredient-product-name">Продукт</FieldLabel>
        <div className="relative">
          <Search className="text-muted-foreground pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2" />
          <Input
            id="ingredient-product-name"
            className="pl-9"
            value={value.productName}
            placeholder="Например, рис или куриное филе"
            autoComplete="off"
            aria-invalid={Boolean(errors.productName)}
            onChange={(event) => {
              update({
                productName: event.target.value,
                ingredientId: "",
              })
            }}
          />
        </div>

        {suggestions.length > 0 ? (
          <div className="bg-popover text-popover-foreground divide-y rounded-lg border shadow-sm">
            {suggestions.map((product) => (
              <button
                key={product.id}
                type="button"
                className="type-body hover:bg-muted focus-visible:bg-muted flex w-full items-center justify-between gap-3 px-3 py-2.5 text-left outline-none"
                onClick={() => {
                  update({
                    ingredientId: product.id,
                    productName: product.name,
                    category: product.category,
                  })
                }}
              >
                <span className="font-medium">{product.name}</span>
                <span className="type-supporting text-muted-foreground">
                  {getProductCategoryLabel(product.category)}
                </span>
              </button>
            ))}
          </div>
        ) : null}

        {errors.productName ? <FieldError>{errors.productName}</FieldError> : null}
      </Field>

      {value.ingredientId ? (
        <div className="bg-primary/5 flex items-start gap-3 rounded-lg border p-3">
          <CircleCheck className="text-primary mt-0.5 size-4 shrink-0" />
          <div className="min-w-0">
            <div className="type-label">Продукт из общего каталога</div>
            <div className="type-supporting text-muted-foreground">
              Категория: {getProductCategoryLabel(value.category)}
            </div>
          </div>
        </div>
      ) : (
        <Field data-editor-invalid={Boolean(errors.category)}>
          <FieldLabel htmlFor="ingredient-category">Категория нового продукта</FieldLabel>
          <Select
            value={value.category}
            onValueChange={(category) => {
              update({ category })
            }}
          >
            <SelectTrigger
              id="ingredient-category"
              className="w-full"
              aria-invalid={Boolean(errors.category)}
            >
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {productCategoryOptions.map((option) => (
                <SelectItem key={option.value} value={option.value}>
                  {option.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          <FieldDescription>
            После сохранения продукт появится в общем каталоге.
          </FieldDescription>
          {errors.category ? <FieldError>{errors.category}</FieldError> : null}
        </Field>
      )}

      <section className="space-y-3">
        <div>
          <h3 className="type-subsection-title">Количество</h3>
          <p className="type-supporting text-muted-foreground">
            Укажите точное, примерное количество или выберите «по вкусу».
          </p>
        </div>

        <div className="bg-muted/50 grid grid-cols-3 gap-1 rounded-lg border p-1" role="radiogroup" aria-label="Тип количества">
          {quantityKindOptions.map((option) => {
            const isSelected = option.value === value.quantityKind

            return (
              <button
                key={option.value}
                type="button"
                role="radio"
                aria-checked={isSelected}
                className={
                  isSelected
                    ? "bg-background type-label rounded-md px-2 py-2 shadow-sm"
                    : "type-label text-muted-foreground hover:text-foreground rounded-md px-2 py-2"
                }
                onClick={() => {
                  update({ quantityKind: option.value })
                }}
              >
                {option.label}
              </button>
            )
          })}
        </div>

        {value.quantityKind === "ToTaste" ? (
          <div className="bg-primary/5 flex items-center gap-2 rounded-lg border p-3">
            <Sparkles className="text-primary size-4" />
            <span className="type-supporting">Числовое количество для этого ингредиента не требуется.</span>
          </div>
        ) : (
          <div className="grid grid-cols-[minmax(0,1fr)_minmax(8rem,0.8fr)] gap-3">
            <Field data-editor-invalid={Boolean(errors.amount)}>
              <FieldLabel htmlFor="ingredient-amount">Значение</FieldLabel>
              <Input
                id="ingredient-amount"
                inputMode="decimal"
                value={value.amount}
                placeholder="0"
                aria-invalid={Boolean(errors.amount)}
                onChange={(event) => {
                  update({ amount: event.target.value })
                }}
              />
              {errors.amount ? <FieldError>{errors.amount}</FieldError> : null}
            </Field>

            <Field data-editor-invalid={Boolean(errors.unit)}>
              <FieldLabel htmlFor="ingredient-unit">Единица</FieldLabel>
              <Select
                value={value.unit}
                onValueChange={(unit) => {
                  update({ unit })
                }}
              >
                <SelectTrigger
                  id="ingredient-unit"
                  className="w-full"
                  aria-invalid={Boolean(errors.unit)}
                >
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {measurementUnitOptions
                    .filter((option) => option.value !== "ToTaste")
                    .map((option) => (
                      <SelectItem key={option.value} value={option.value}>
                        {option.label}
                      </SelectItem>
                    ))}
                </SelectContent>
              </Select>
              {errors.unit ? <FieldError>{errors.unit}</FieldError> : null}
            </Field>
          </div>
        )}
      </section>

      <Field>
        <FieldLabel htmlFor="ingredient-comment">Описание или подготовка</FieldLabel>
        <Textarea
          id="ingredient-comment"
          className="min-h-20"
          value={value.comment}
          placeholder="Например, промыть, мелко нарезать или заменить по желанию"
          onChange={(event) => {
            update({ comment: event.target.value })
          }}
        />
        <FieldDescription>
          Эта подсказка будет видна рядом с ингредиентом в рецепте.
        </FieldDescription>
      </Field>

      <label className="has-data-[state=checked]:border-primary has-data-[state=checked]:bg-primary/5 flex cursor-pointer items-start gap-3 rounded-lg border p-4 transition-colors">
        <Checkbox
          className="mt-0.5"
          checked={value.isOptional}
          onCheckedChange={(checked) => {
            update({ isOptional: checked === true })
          }}
        />
        <span className="min-w-0">
          <span className="type-label block">Можно пропустить</span>
          <span className="type-supporting text-muted-foreground mt-0.5 block">
            Блюдо можно приготовить и без этого ингредиента.
          </span>
        </span>
      </label>
    </div>
  )
}
