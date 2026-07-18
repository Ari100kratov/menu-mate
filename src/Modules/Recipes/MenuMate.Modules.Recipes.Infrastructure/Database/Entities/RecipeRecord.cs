using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.Modules.Recipes.Domain.Models;
using MenuMate.Modules.Recipes.Domain.ValueObjects;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.Recipes.Infrastructure.Database.Entities;

internal sealed class RecipeRecord
{
    public Guid Id { get; set; }
    public UserId OwnerUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Servings { get; set; }
    public RecipeCategory Category { get; set; }
    public RecipeVisibility Visibility { get; set; }
    public Guid CurrentRevisionId { get; set; }
    public int RevisionNumber { get; set; }
    public Guid? SourceRecipeId { get; set; }
    public RecipeRevisionId? SourceRevisionId { get; set; }
    public int? TotalTimeMinutes { get; set; }
    public int? ActiveTimeMinutes { get; set; }
    public string? SourceUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public List<RecipeIngredientRecord> Ingredients { get; set; } = [];
    public List<PreparationStepRecord> Steps { get; set; } = [];
    public List<RecipeImageRecord> Images { get; set; } = [];
    public List<RecipeRevisionRecord> Revisions { get; set; } = [];
    public List<RecipeLibraryEntryRecord> LibraryEntries { get; set; } = [];

    public static RecipeRecord FromDomain(Recipe recipe)
    {
        var record = new RecipeRecord();
        record.Apply(recipe);
        return record;
    }

    public void Apply(Recipe recipe)
    {
        Id = recipe.Id;
        OwnerUserId = recipe.OwnerUserId;
        Title = recipe.Title.Value;
        Description = recipe.Description;
        Servings = recipe.Servings.Value;
        Category = recipe.Category;
        Visibility = recipe.Visibility;
        CurrentRevisionId = recipe.CurrentRevisionId.Value;
        RevisionNumber = recipe.RevisionNumber;
        SourceRecipeId = recipe.SourceRecipeId;
        SourceRevisionId = recipe.SourceRevisionId;
        TotalTimeMinutes = recipe.TotalTimeMinutes;
        ActiveTimeMinutes = recipe.ActiveTimeMinutes;
        SourceUrl = recipe.SourceUrl?.ToString();
        CreatedAt = recipe.CreatedAt;
        UpdatedAt = recipe.UpdatedAt;
        IsDeleted = recipe.IsDeleted;

        Ingredients.Clear();
        Ingredients.AddRange(recipe.Ingredients.Select(RecipeIngredientRecord.FromDomain));
        Steps.Clear();
        Steps.AddRange(recipe.Steps.Select(PreparationStepRecord.FromDomain));

        foreach (RecipeIngredientRecord ingredient in Ingredients)
        {
            ingredient.RecipeId = Id;
        }

        foreach (PreparationStepRecord step in Steps)
        {
            step.RecipeId = Id;
        }

        if (Revisions.All(revision => revision.Id != recipe.CurrentRevisionId.Value))
        {
            Revisions.Add(RecipeRevisionRecord.FromDomain(recipe));
        }
    }

    public Recipe ToDomain(IReadOnlyCollection<RecipeTag> tags)
    {
        Result<RecipeTitle> title = RecipeTitle.Create(Title);
        Result<Servings> servings = Domain.ValueObjects.Servings.Create(Servings);
        if (title.IsFailure)
        {
            throw new DomainException(title.Error);
        }

        if (servings.IsFailure)
        {
            throw new DomainException(servings.Error);
        }

        Uri? sourceUrl = SourceUrl is null ? null : new Uri(SourceUrl, UriKind.Absolute);
        return Recipe.Rehydrate(
            Id,
            OwnerUserId,
            title.Value,
            servings.Value,
            Category,
            Visibility,
            RecipeRevisionId.From(CurrentRevisionId),
            RevisionNumber,
            SourceRecipeId,
            SourceRevisionId,
            TotalTimeMinutes,
            ActiveTimeMinutes,
            Description,
            sourceUrl,
            CreatedAt,
            UpdatedAt,
            IsDeleted,
            Ingredients.OrderBy(ingredient => ingredient.Order).Select(ingredient => ingredient.ToDomain()),
            Steps.OrderBy(step => step.Number).Select(step => step.ToDomain()),
            tags);
    }
}
