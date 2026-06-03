using MenuMate.Common.Application;

namespace MenuMate.Modules.Recipes.Application.DeleteRecipeImage;

/// <summary>
/// Команда удаления изображения рецепта.
/// </summary>
internal sealed record DeleteRecipeImageCommand(Guid RecipeId, Guid ImageId) : ICommand;
