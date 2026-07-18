using MenuMate.Common.Application.Tags;
using MenuMate.Modules.Recipes.Domain.ValueObjects;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Application;

internal sealed class RecipeTagCatalogResolver(ITagCatalog tagCatalog)
{
    public async Task<Result<IReadOnlyCollection<RecipeTag>>> ResolveAsync(
        IReadOnlyCollection<string> names,
        TagCatalogSource source,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<TagCatalogItem> catalogItems = await tagCatalog.ResolveAsync(
            names,
            source,
            cancellationToken);
        var tags = new List<RecipeTag>(catalogItems.Count);

        foreach (TagCatalogItem item in catalogItems)
        {
            Result<RecipeTag> tag = RecipeTag.Create(item.Id, item.Name);
            if (tag.IsFailure)
            {
                return Result.Failure<IReadOnlyCollection<RecipeTag>>(tag.Error);
            }

            tags.Add(tag.Value);
        }

        return tags;
    }
}
