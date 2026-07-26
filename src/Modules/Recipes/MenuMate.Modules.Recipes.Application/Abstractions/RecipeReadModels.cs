using MenuMate.Contracts.Recipes;

namespace MenuMate.Modules.Recipes.Application.Abstractions;

internal sealed record RecipeReadModel(
    RecipeResponse Response,
    IReadOnlyCollection<Guid> TagIds);

internal sealed record RecipeListItemReadModel(
    RecipeListItemResponse Response,
    IReadOnlyCollection<Guid> TagIds);

internal sealed record RecipeListPageReadModel(
    IReadOnlyCollection<RecipeListItemReadModel> Items,
    int TotalCount);

internal sealed record RecipeRevisionAccessReadModel(bool IsSourceAccessible);
