namespace MenuMate.Modules.Recipes.Domain.Enums;

/// <summary>
/// Controls who can read and save a recipe.
/// </summary>
public enum RecipeVisibility
{
    /// <summary>Only the owner can read the recipe.</summary>
    Private = 0,
    /// <summary>Any authenticated user can read and save the recipe.</summary>
    Public = 1
}
