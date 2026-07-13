import { CircleCheck, CopyPlus, Search } from "lucide-react"

import { useProductsQuery } from "@/features/products/api/products.queries"
import type {
  ProductLineEditorErrors,
  ProductLineEditorValue,
} from "@/features/products/model/product-line"
import {
  getProductCategoryLabel,
  measurementUnitOptions,
  productCategoryOptions,
} from "@/shared/config/product-taxonomy"
import { Button } from "@/shared/ui/button"
import { Checkbox } from "@/shared/ui/checkbox"
import { Field, FieldDescription, FieldError, FieldLabel } from "@/shared/ui/field"
import { Input } from "@/shared/ui/input"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/shared/ui/select"
import { Textarea } from "@/shared/ui/textarea"

interface ProductLineEditorFieldsProps {
  idPrefix: string
  value: ProductLineEditorValue
  errors: ProductLineEditorErrors
  commentLabel?: string
  commentPlaceholder?: string
  commentDescription?: string
  onChange: (value: ProductLineEditorValue) => void
}

export function ProductLineEditorFields({
  idPrefix,
  value,
  errors,
  commentLabel = "Комментарий",
  commentPlaceholder = "Например, выбрать спелый или заменить по желанию",
  commentDescription = "Комментарий будет виден рядом с продуктом.",
  onChange,
}: ProductLineEditorFieldsProps) {
  const productsQuery = useProductsQuery(value.productName)
  const suggestions =
    value.productName.trim() && !value.productId ? (productsQuery.data ?? []).slice(0, 6) : []
  const normalizedProductName = normalizeProductName(value.productName)
  const exactNameMatches = (productsQuery.data ?? []).filter(
    (product) => normalizeProductName(product.name) === normalizedProductName,
  )
  const exactCategoryMatch = exactNameMatches.find((product) => product.category === value.category)

  function update(patch: Partial<ProductLineEditorValue>) {
    onChange({ ...value, ...patch })
  }

  return (
    <div className="space-y-6">
      <Field data-editor-invalid={Boolean(errors.productName)}>
        <FieldLabel htmlFor={`${idPrefix}-product-name`}>Продукт</FieldLabel>
        <div className="relative">
          <Search className="text-muted-foreground pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2" />
          <Input
            id={`${idPrefix}-product-name`}
            className="pl-9"
            value={value.productName}
            placeholder="Например, рис или куриное филе"
            autoComplete="off"
            aria-invalid={Boolean(errors.productName)}
            onChange={(event) => {
              update({ productName: event.target.value, productId: "" })
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
                    productId: product.id,
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

      {value.productId ? (
        <div className="bg-primary/5 flex items-start gap-3 rounded-lg border p-3">
          <CircleCheck className="text-primary mt-0.5 size-4 shrink-0" />
          <div className="min-w-0">
            <div className="type-label">Продукт из общего каталога</div>
            <div className="type-supporting text-muted-foreground">
              Категория: {getProductCategoryLabel(value.category)}
            </div>
            <Button
              type="button"
              variant="link"
              size="sm"
              className="mt-1 h-auto justify-start p-0"
              onClick={() => {
                update({ productId: "" })
              }}
            >
              <CopyPlus />
              Создать вариант в другой категории
            </Button>
          </div>
        </div>
      ) : (
        <Field data-editor-invalid={Boolean(errors.category)}>
          <FieldLabel htmlFor={`${idPrefix}-category`}>Категория нового продукта</FieldLabel>
          <Select
            value={value.category}
            onValueChange={(category) => {
              update({ category })
            }}
          >
            <SelectTrigger
              id={`${idPrefix}-category`}
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
          {exactNameMatches.length > 0 ? (
            <FieldDescription>
              {exactCategoryMatch
                ? `В категории «${getProductCategoryLabel(value.category)}» такой продукт уже есть. При сохранении будет использована существующая позиция каталога. Выберите другую категорию, чтобы создать отдельный вариант.`
                : `Будет создан отдельный вариант «${value.productName.trim()}» в категории «${getProductCategoryLabel(value.category)}».`}
            </FieldDescription>
          ) : (
            <FieldDescription>После сохранения продукт появится в общем каталоге.</FieldDescription>
          )}
          {errors.category ? <FieldError>{errors.category}</FieldError> : null}
        </Field>
      )}

      <section className="space-y-3">
        <div>
          <h3 className="type-subsection-title">Количество</h3>
        </div>

        <label className="has-data-[state=checked]:border-primary has-data-[state=checked]:bg-primary/5 flex cursor-pointer items-start gap-3 rounded-lg border p-3 transition-colors">
          <Checkbox
            className="mt-0.5"
            checked={value.isToTaste}
            onCheckedChange={(checked) => {
              const isToTaste = checked === true
              update({
                isToTaste,
                amount: isToTaste ? "" : value.amount,
                unit: isToTaste ? "ToTaste" : value.unit === "ToTaste" ? "Unknown" : value.unit,
              })
            }}
          />
          <span className="min-w-0">
            <span className="type-label block">По вкусу</span>
            <span className="type-supporting text-muted-foreground mt-0.5 block">
              Числовое количество и единица измерения не нужны.
            </span>
          </span>
        </label>

        {value.isToTaste ? null : (
          <div className="grid grid-cols-[minmax(0,1fr)_minmax(8rem,0.8fr)] gap-3">
            <Field data-editor-invalid={Boolean(errors.amount)}>
              <FieldLabel htmlFor={`${idPrefix}-amount`}>Значение</FieldLabel>
              <Input
                id={`${idPrefix}-amount`}
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
              <FieldLabel htmlFor={`${idPrefix}-unit`}>Единица</FieldLabel>
              <Select
                value={value.unit}
                onValueChange={(unit) => {
                  update({ unit })
                }}
              >
                <SelectTrigger
                  id={`${idPrefix}-unit`}
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
        <FieldLabel htmlFor={`${idPrefix}-comment`}>{commentLabel}</FieldLabel>
        <Textarea
          id={`${idPrefix}-comment`}
          className="min-h-20"
          value={value.comment}
          placeholder={commentPlaceholder}
          onChange={(event) => {
            update({ comment: event.target.value })
          }}
        />
        <FieldDescription>{commentDescription}</FieldDescription>
      </Field>
    </div>
  )
}

function normalizeProductName(value: string) {
  return value.trim().toLocaleLowerCase("ru-RU").replaceAll(/\s+/g, " ")
}
