using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.Modules.Recipes.Domain.Models;
using MenuMate.Modules.Recipes.Domain.ValueObjects;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.Recipes.Domain.UnitTests.Models;

public sealed class RecipeTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 6, 12, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateShouldInitializeFirstRevisionAndSourceMetadata()
    {
        var sourceRecipeId = Guid.CreateVersion7();
        var sourceRevisionId = RecipeRevisionId.Create();

        Recipe recipe = CreateRecipe(sourceRecipeId, sourceRevisionId);

        Assert.Equal(1, recipe.RevisionNumber);
        Assert.NotEqual(default, recipe.CurrentRevisionId);
        Assert.Equal(sourceRecipeId, recipe.SourceRecipeId);
        Assert.Equal(sourceRevisionId, recipe.SourceRevisionId);
        Assert.False(recipe.IsDeleted);
    }

    [Fact]
    public void UpdateDetailsShouldReplaceScalarContentAndTrimDescription()
    {
        Recipe recipe = CreateRecipe();
        DateTimeOffset updatedAt = FixedNow.AddMinutes(1);
        Uri sourceUrl = new("https://example.com/recipe");

        recipe.UpdateDetails(
            RecipeTitle.Create("Обновленная паста").Value,
            Servings.Create(4).Value,
            RecipeCategory.MainCourse,
            RecipeVisibility.Public,
            45,
            20,
            "  Быстрый ужин  ",
            sourceUrl,
            updatedAt);

        Assert.Equal("Обновленная паста", recipe.Title.Value);
        Assert.Equal(4, recipe.Servings.Value);
        Assert.Equal(RecipeVisibility.Public, recipe.Visibility);
        Assert.Equal(45, recipe.TotalTimeMinutes);
        Assert.Equal(20, recipe.ActiveTimeMinutes);
        Assert.Equal("Быстрый ужин", recipe.Description);
        Assert.Equal(sourceUrl, recipe.SourceUrl);
        Assert.Equal(updatedAt, recipe.UpdatedAt);
    }

    [Fact]
    public void ReplaceStepsShouldSortStepsByNumber()
    {
        Recipe recipe = CreateRecipe();

        recipe.ReplaceSteps(
            [
                PreparationStep.Create(2, "Подать").Value,
                PreparationStep.Create(1, "Приготовить").Value
            ],
            FixedNow.AddMinutes(1));

        Assert.Equal([1, 2], recipe.Steps.Select(step => step.Number));
    }

    [Fact]
    public void ReplaceTagsShouldDeduplicateByGlobalTagId()
    {
        Recipe recipe = CreateRecipe();
        var fastTagId = Guid.CreateVersion7();

        recipe.ReplaceTags(
            [
                RecipeTag.Create(fastTagId, " Быстро ").Value,
                RecipeTag.Create(fastTagId, "быстро").Value,
                RecipeTag.Create(Guid.CreateVersion7(), "Ужин").Value
            ],
            FixedNow.AddMinutes(1));

        Assert.Equal(["БЫСТРО", "УЖИН"], recipe.Tags.Select(tag => tag.NormalizedValue));
    }

    [Fact]
    public void AddAndRemoveTagShouldUseGlobalTagId()
    {
        Recipe recipe = CreateRecipe();
        var tagId = Guid.CreateVersion7();
        RecipeTag tag = RecipeTag.Create(tagId, "Быстро").Value;

        recipe.AddTag(tag, FixedNow.AddMinutes(1));
        recipe.AddTag(RecipeTag.Create(tagId, " быстро ").Value, FixedNow.AddMinutes(2));
        recipe.RemoveTag(tagId, FixedNow.AddMinutes(3));

        Assert.Empty(recipe.Tags);
        Assert.Equal(FixedNow.AddMinutes(3), recipe.UpdatedAt);
    }

    [Fact]
    public void ScaleIngredientsForShouldReturnScaledCopyWithoutChangingRecipe()
    {
        Recipe recipe = CreateRecipe();
        recipe.ReplaceIngredients(
            [
                new RecipeIngredient(
                    Guid.CreateVersion7(),
                    IngredientName.Create("Молоко").Value,
                    IngredientQuantity.Measured(250m, MeasurementUnit.Milliliter).Value,
                    ProductCategory.Dairy,
                    null,
                    false)
            ],
            FixedNow);

        RecipeIngredient scaled = Assert.Single(recipe.ScaleIngredientsFor(Servings.Create(4).Value));

        Assert.Equal(500m, scaled.Quantity.Amount);
        Assert.Equal(250m, Assert.Single(recipe.Ingredients).Quantity.Amount);
    }

    [Fact]
    public void PublishRevisionAndDeleteShouldAdvanceState()
    {
        Recipe recipe = CreateRecipe();
        RecipeRevisionId initialRevisionId = recipe.CurrentRevisionId;

        recipe.PublishRevision(FixedNow.AddMinutes(1));
        recipe.Delete(FixedNow.AddMinutes(2));

        Assert.Equal(2, recipe.RevisionNumber);
        Assert.NotEqual(initialRevisionId, recipe.CurrentRevisionId);
        Assert.True(recipe.IsDeleted);
        Assert.Equal(FixedNow.AddMinutes(2), recipe.UpdatedAt);
    }

    private static Recipe CreateRecipe(
        Guid? sourceRecipeId = null,
        RecipeRevisionId? sourceRevisionId = null) =>
        Recipe.Create(
            Guid.CreateVersion7(),
            UserId.Create(),
            RecipeTitle.Create("Паста").Value,
            Servings.Create(2).Value,
            RecipeCategory.MainCourse,
            RecipeVisibility.Private,
            FixedNow,
            sourceRecipeId,
            sourceRevisionId);
}
