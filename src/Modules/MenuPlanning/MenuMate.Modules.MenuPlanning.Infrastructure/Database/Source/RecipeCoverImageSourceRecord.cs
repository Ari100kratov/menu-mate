using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.MenuPlanning.Infrastructure.Database.Source;

internal sealed class RecipeCoverImageSourceRecord
{
    public Guid Id { get; set; }
    public RecipeId RecipeId { get; set; }
    public string Scope { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string ObjectKey { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
