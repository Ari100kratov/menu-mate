using MenuMate.Common.Application;
using MenuMate.Contracts.Recipes;
using MenuMate.Modules.Recipes.Application.CreateRecipe;
using MenuMate.Modules.Recipes.Application.CopyRecipe;
using MenuMate.Modules.Recipes.Application.DeleteRecipe;
using MenuMate.Modules.Recipes.Application.DeleteRecipeImage;
using MenuMate.Modules.Recipes.Application.GetRecipeById;
using MenuMate.Modules.Recipes.Application.GetRecipes;
using MenuMate.Modules.Recipes.Application.RecipeImages;
using MenuMate.Modules.Recipes.Application.SetRecipeFavorite;
using MenuMate.Modules.Recipes.Application.SetRecipeLibrary;
using MenuMate.Modules.Recipes.Application.UpdateRecipe;
using MenuMate.Modules.Recipes.Application.UploadRecipeImage;
using Microsoft.Extensions.DependencyInjection;

namespace MenuMate.Modules.Recipes.Application;

/// <summary>
/// Регистрация прикладного слоя модуля рецептов.
/// </summary>
public static class RecipesApplicationDependencyInjection
{
    /// <summary>
    /// Добавляет обработчики use case-ов Recipes.
    /// </summary>
    public static IServiceCollection AddRecipesApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<CreateRecipeCommand, RecipeResponse>, CreateRecipeCommandHandler>();
        services.AddScoped<ICommandHandler<CopyRecipeCommand, RecipeResponse>, CopyRecipeCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateRecipeCommand>, UpdateRecipeCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteRecipeCommand>, DeleteRecipeCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteRecipeImageCommand>, DeleteRecipeImageCommandHandler>();
        services.AddScoped<ICommandHandler<SetRecipeFavoriteCommand>, SetRecipeFavoriteCommandHandler>();
        services.AddScoped<ICommandHandler<SetRecipeLibraryCommand>, SetRecipeLibraryCommandHandler>();
        services.AddScoped<ICommandHandler<UploadRecipeImageCommand, RecipeImageResponse>, UploadRecipeImageCommandHandler>();
        services.AddScoped<IQueryHandler<GetRecipeByIdQuery, RecipeResponse>, GetRecipeByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetRecipesQuery, IReadOnlyCollection<RecipeListItemResponse>>, GetRecipesQueryHandler>();
        services.AddScoped<RecipeImageReadUrlService>();
        services.AddScoped<RecipeProductResolver>();

        return services;
    }
}
