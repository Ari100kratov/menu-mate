import { ArrowLeft } from "lucide-react"
import { Link, useNavigate } from "react-router-dom"

import { useCreateRecipeMutation } from "@/features/recipes/api/recipes.queries"
import {
  createEmptyRecipeFormValues,
  toRecipeRequest,
  type RecipeFormValues,
} from "@/features/recipes/model/recipe-form"
import { RecipeForm } from "@/features/recipes/ui/RecipeForm"
import { Button } from "@/shared/ui/button"
import { PageHeader } from "@/shared/ui/page"

export default function RecipeCreatePage() {
  const navigate = useNavigate()
  const createRecipeMutation = useCreateRecipeMutation()

  function handleSubmit(values: RecipeFormValues) {
    createRecipeMutation.mutate(toRecipeRequest(values), {
      onSuccess: (recipe) => {
        void navigate(`/recipes/${recipe.id}/edit`, { replace: true })
      },
    })
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Новый рецепт"
        description="Сохраните рецепт с ингредиентами, шагами и тегами."
        action={
          <Button asChild variant="outline">
            <Link to="/recipes">
              <ArrowLeft />К списку
            </Link>
          </Button>
        }
      />

      <RecipeForm
        initialValues={createEmptyRecipeFormValues()}
        submitLabel="Создать рецепт"
        isSubmitting={createRecipeMutation.isPending}
        error={createRecipeMutation.error}
        onSubmit={handleSubmit}
      />
    </div>
  )
}
