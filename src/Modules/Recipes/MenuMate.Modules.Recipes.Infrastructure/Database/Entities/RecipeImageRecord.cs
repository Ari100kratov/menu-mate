using MenuMate.Modules.Recipes.Application.UploadRecipeImage;
using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.Recipes.Infrastructure.Database.Entities;

internal sealed class RecipeImageRecord
{
    public Guid Id { get; set; }

    public UserId OwnerUserId { get; set; }

    public Guid RecipeId { get; set; }

    public RecipeImageScope Scope { get; set; }

    public int? StepNumber { get; set; }

    public string BucketName { get; set; } = string.Empty;

    public string ObjectKey { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public string? OriginalFileName { get; set; }

    public string? AltText { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public static RecipeImageRecord FromMetadata(RecipeImageMetadata image) =>
        new()
        {
            Id = image.Id,
            OwnerUserId = image.OwnerUserId,
            RecipeId = image.RecipeId,
            Scope = image.Scope,
            StepNumber = image.StepNumber,
            BucketName = image.BucketName,
            ObjectKey = image.ObjectKey,
            ContentType = image.ContentType,
            SizeBytes = image.SizeBytes,
            OriginalFileName = image.OriginalFileName,
            AltText = image.AltText,
            CreatedAt = image.CreatedAt
        };
}
