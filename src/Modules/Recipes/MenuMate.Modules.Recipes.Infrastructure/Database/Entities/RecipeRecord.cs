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

    public bool IsFavorite { get; set; }

    public string? SourceUrl { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public List<RecipeIngredientRecord> Ingredients { get; set; } = [];

    public List<PreparationStepRecord> Steps { get; set; } = [];

    public List<RecipeTagRecord> Tags { get; set; } = [];

    public List<RecipeImageRecord> Images { get; set; } = [];

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
        IsFavorite = recipe.IsFavorite;
        SourceUrl = recipe.SourceUrl?.ToString();
        CreatedAt = recipe.CreatedAt;
        UpdatedAt = recipe.UpdatedAt;
        IsDeleted = recipe.IsDeleted;
        Ingredients.Clear();
        Ingredients.AddRange(recipe.Ingredients.Select(RecipeIngredientRecord.FromDomain));

        Steps.Clear();
        Steps.AddRange(recipe.Steps.Select(PreparationStepRecord.FromDomain));

        Tags.Clear();
        Tags.AddRange(recipe.Tags.Select(RecipeTagRecord.FromDomain));

        foreach (RecipeIngredientRecord ingredient in Ingredients)
        {
            ingredient.RecipeId = Id;
        }

        foreach (PreparationStepRecord step in Steps)
        {
            step.RecipeId = Id;
        }

        foreach (RecipeTagRecord tag in Tags)
        {
            tag.RecipeId = Id;
        }
    }

    public Recipe ToDomain()
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
            Description,
            IsFavorite,
            sourceUrl,
            CreatedAt,
            UpdatedAt,
            IsDeleted,
            Ingredients.OrderBy(ingredient => ingredient.Order).Select(ingredient => ingredient.ToDomain()),
            Steps.OrderBy(step => step.Number).Select(step => step.ToDomain()),
            Tags.Select(tag => tag.ToDomain()));
    }
}
