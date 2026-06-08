namespace MenuMate.Contracts.Products;

/// <summary>
/// Продукт из общего каталога.
/// </summary>
public sealed record ProductResponse(Guid Id, string Name, string Category);
