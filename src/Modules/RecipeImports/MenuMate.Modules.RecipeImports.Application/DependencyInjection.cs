using MenuMate.Common.Application;
using MenuMate.Contracts.RecipeImports;
using MenuMate.Contracts.Recipes;
using MenuMate.Modules.RecipeImports.Application.ConfirmRecipeImportDraft;
using MenuMate.Modules.RecipeImports.Application.CreateRecipeImportDraft;
using MenuMate.Modules.RecipeImports.Application.DeleteRecipeImportDraft;
using MenuMate.Modules.RecipeImports.Application.GetRecipeImportDraft;
using MenuMate.Modules.RecipeImports.Application.GetRecipeImportDrafts;
using MenuMate.Modules.RecipeImports.Application.GetRecipeImportSourceImage;
using MenuMate.Modules.RecipeImports.Application.GenerateRecipeCoverImage;
using MenuMate.Modules.RecipeImports.Application.Generation;
using MenuMate.Modules.RecipeImports.Application.UpdateRecipeImportDraft;
using Microsoft.Extensions.DependencyInjection;

namespace MenuMate.Modules.RecipeImports.Application;

/// <summary>
/// Регистрация прикладного слоя модуля Imports.
/// </summary>
public static class RecipeImportsApplicationDependencyInjection
{
    /// <summary>Добавляет сценарии использования импорта рецептов.</summary>
    public static IServiceCollection AddRecipeImportsApplication(this IServiceCollection services)
    {
        services.AddScoped<RecipeImportDraftMapping>();
        services.AddScoped<ICommandHandler<CreateRecipeImportDraftCommand, RecipeImportDraftResponse>, CreateRecipeImportDraftCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateRecipeImportDraftCommand, RecipeImportDraftResponse>, UpdateRecipeImportDraftCommandHandler>();
        services.AddScoped<ICommandHandler<ConfirmRecipeImportDraftCommand, RecipeResponse>, ConfirmRecipeImportDraftCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteRecipeImportDraftCommand>, DeleteRecipeImportDraftCommandHandler>();
        services.AddScoped<ICommandHandler<GenerateRecipeCoverImageCommand, GeneratedRecipeCoverImage>, GenerateRecipeCoverImageCommandHandler>();
        services.AddScoped<IQueryHandler<GetRecipeImportDraftQuery, RecipeImportDraftResponse>, GetRecipeImportDraftQueryHandler>();
        services.AddScoped<IQueryHandler<GetRecipeImportDraftsQuery, IReadOnlyCollection<RecipeImportDraftListItemResponse>>, GetRecipeImportDraftsQueryHandler>();
        services.AddScoped<IQueryHandler<GetRecipeImportSourceImageQuery, RecipeImportSourceImageContent>, GetRecipeImportSourceImageQueryHandler>();
        return services;
    }
}
