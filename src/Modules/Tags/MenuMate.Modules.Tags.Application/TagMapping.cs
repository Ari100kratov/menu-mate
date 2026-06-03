using MenuMate.Contracts.Tags;
using MenuMate.Modules.Tags.Domain.Models;

namespace MenuMate.Modules.Tags.Application;

internal static class TagMapping
{
    public static TagResponse ToResponse(Tag tag) =>
        new(
            tag.Id,
            tag.Name.Value,
            tag.Name.NormalizedValue,
            tag.Kind.ToString(),
            tag.Status.ToString(),
            tag.CreatedAt,
            tag.UpdatedAt);
}
