using MenuMate.Common.Application;
using MenuMate.Contracts.Recipes;
using MenuMate.Modules.RecipeImports.Application.Generation;

namespace MenuMate.Modules.RecipeImports.Application.GenerateRecipeCoverImage;

/// <summary>Команда генерации обложки по данным рецепта.</summary>
public sealed record GenerateRecipeCoverImageCommand(
    CreateRecipeRequest Recipe) : ICommand<GeneratedRecipeCoverImage>;
