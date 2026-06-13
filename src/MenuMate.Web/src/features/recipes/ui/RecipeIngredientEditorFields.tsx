import type {
  ProductLineEditorErrors,
  ProductLineEditorValue,
} from "@/features/products/model/product-line"
import { ProductLineEditorFields } from "@/features/products/ui/ProductLineEditorFields"
import type {
  RecipeIngredientFieldErrors,
  RecipeIngredientFormValues,
} from "@/features/recipes/model/recipe-form"
import { Checkbox } from "@/shared/ui/checkbox"

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
  const productLineValue = toProductLineValue(value)

  return (
    <div className="space-y-6">
      <ProductLineEditorFields
        idPrefix="ingredient"
        value={productLineValue}
        errors={toProductLineErrors(errors)}
        commentLabel="Описание или подготовка"
        commentPlaceholder="Например, промыть, мелко нарезать или заменить по желанию"
        commentDescription="Эта подсказка будет видна рядом с ингредиентом в рецепте."
        onChange={(nextValue) => {
          const { productId, ...ingredientValue } = nextValue
          onChange({
            ...value,
            ...ingredientValue,
            ingredientId: productId,
          })
        }}
      />

      <label className="has-data-[state=checked]:border-primary has-data-[state=checked]:bg-primary/5 flex cursor-pointer items-start gap-3 rounded-lg border p-4 transition-colors">
        <Checkbox
          className="mt-0.5"
          checked={value.isOptional}
          onCheckedChange={(checked) => {
            onChange({ ...value, isOptional: checked === true })
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

function toProductLineValue(value: RecipeIngredientFormValues): ProductLineEditorValue {
  return {
    productId: value.ingredientId,
    productName: value.productName,
    amount: value.amount,
    unit: value.unit,
    isToTaste: value.isToTaste,
    category: value.category,
    comment: value.comment,
  }
}

function toProductLineErrors(errors: RecipeIngredientFieldErrors): ProductLineEditorErrors {
  return {
    productId: errors.ingredientId,
    productName: errors.productName,
    amount: errors.amount,
    unit: errors.unit,
    isToTaste: errors.isToTaste,
    category: errors.category,
    comment: errors.comment,
  }
}
