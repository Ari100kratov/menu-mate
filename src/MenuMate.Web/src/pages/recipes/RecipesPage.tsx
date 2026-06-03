import { Plus } from "lucide-react"
import { useMemo, useState } from "react"
import { Link } from "react-router-dom"

import { getTagOptions } from "@/features/recipes/model/recipe-list-filters"
import { RecipeCard } from "@/features/recipes/ui/RecipeCard"
import { RecipeFiltersSection } from "@/features/recipes/ui/RecipeFiltersSection"
import {
  useDeleteRecipeMutation,
  useRecipesQuery,
  useSetRecipeFavoriteMutation,
} from "@/features/recipes/api/recipes.queries"
import { useTagsQuery } from "@/features/tags/api/tags.queries"
import { Button } from "@/shared/ui/button"
import { ErrorAlert, PageSkeleton } from "@/shared/ui/feedback"
import { EmptyState, PageHeader } from "@/shared/ui/page"

export default function RecipesPage() {
  const [search, setSearch] = useState("")
  const [tag, setTag] = useState("")
  const [favoritesOnly, setFavoritesOnly] = useState(false)
  const recipesQuery = useRecipesQuery({ search, tag, favoritesOnly })
  const tagsQuery = useTagsQuery({ search: "", includeHidden: false })
  const deleteRecipeMutation = useDeleteRecipeMutation()
  const favoriteMutation = useSetRecipeFavoriteMutation()
  const tagOptions = useMemo(() => getTagOptions(tagsQuery.data ?? [], tag), [tag, tagsQuery.data])

  function resetFilters() {
    setSearch("")
    setTag("")
    setFavoritesOnly(false)
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Рецепты"
        description="Рабочий список рецептов с быстрым поиском, избранным и переходом к редактированию."
        action={
          <Button asChild>
            <Link to="/recipes/new">
              <Plus />
              Добавить
            </Link>
          </Button>
        }
      />

      <RecipeFiltersSection
        search={search}
        tag={tag}
        tagOptions={tagOptions}
        favoritesOnly={favoritesOnly}
        recipesCount={recipesQuery.data?.length}
        onSearchChange={setSearch}
        onTagChange={setTag}
        onFavoritesOnlyChange={setFavoritesOnly}
        onReset={resetFilters}
      />

      {recipesQuery.error ? <ErrorAlert error={recipesQuery.error} /> : null}
      {tagsQuery.error ? <ErrorAlert error={tagsQuery.error} /> : null}
      {deleteRecipeMutation.error ? <ErrorAlert error={deleteRecipeMutation.error} /> : null}
      {favoriteMutation.error ? <ErrorAlert error={favoriteMutation.error} /> : null}

      {recipesQuery.isPending ? (
        <PageSkeleton />
      ) : recipesQuery.data && recipesQuery.data.length > 0 ? (
        <section className="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
          {recipesQuery.data.map((recipe) => (
            <RecipeCard
              key={recipe.id}
              recipe={recipe}
              isMutationPending={deleteRecipeMutation.isPending || favoriteMutation.isPending}
              activeTag={tag}
              onDelete={() => {
                if (window.confirm(`Удалить рецепт «${recipe.title}»?`)) {
                  deleteRecipeMutation.mutate(recipe.id)
                }
              }}
              onToggleFavorite={() => {
                favoriteMutation.mutate({
                  recipeId: recipe.id,
                  isFavorite: !recipe.isFavorite,
                })
              }}
              onSelectTag={setTag}
            />
          ))}
        </section>
      ) : (
        <EmptyState
          icon={Plus}
          title="Рецептов пока нет"
          description="Добавьте первый рецепт, чтобы использовать его в меню и списках покупок."
          action={
            <Button asChild>
              <Link to="/recipes/new">
                <Plus />
                Добавить рецепт
              </Link>
            </Button>
          }
        />
      )}
    </div>
  )
}
