using MenuMate.Common.Application;
using MenuMate.Contracts.Recipes;

namespace MenuMate.Modules.Recipes.Application.UploadRecipeImage;

internal sealed record UploadRecipeImageCommand(
    Guid RecipeId,
    Stream Content,
    string FileName,
    string ContentType,
    long ContentLength,
    string? Scope,
    int? StepNumber,
    string? AltText) : ICommand<RecipeImageResponse>;
