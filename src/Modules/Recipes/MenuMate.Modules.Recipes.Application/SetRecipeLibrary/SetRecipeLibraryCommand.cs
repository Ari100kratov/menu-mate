using MenuMate.Common.Application;

namespace MenuMate.Modules.Recipes.Application.SetRecipeLibrary;

/// <summary>
/// Adds or removes a recipe from the current user's library.
/// </summary>
/// <param name="RecipeId">Recipe identifier.</param>
/// <param name="IsSaved">Whether the recipe should be saved.</param>
public sealed record SetRecipeLibraryCommand(Guid RecipeId, bool IsSaved) : ICommand;
