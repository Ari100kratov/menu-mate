namespace MenuMate.Modules.Products.Infrastructure.Database;

internal sealed class ProductRecord
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string NormalizedName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
