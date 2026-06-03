using MenuMate.Common.Application;
using MenuMate.Contracts.Tags;

namespace MenuMate.Modules.Tags.Application.GetTags;

internal sealed record GetTagsQuery(string? Search, bool IncludeHidden) : IQuery<IReadOnlyCollection<TagResponse>>;
