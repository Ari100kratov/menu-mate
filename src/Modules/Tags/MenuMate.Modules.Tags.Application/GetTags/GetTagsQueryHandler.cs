using MenuMate.Common.Application;
using MenuMate.Contracts.Tags;
using MenuMate.Modules.Tags.Application.Abstractions;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Tags.Application.GetTags;

internal sealed class GetTagsQueryHandler(ITagsReadDbContext dbContext)
    : IQueryHandler<GetTagsQuery, IReadOnlyCollection<TagResponse>>
{
    public async Task<Result<IReadOnlyCollection<TagResponse>>> Handle(
        GetTagsQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<TagResponse> tags = await dbContext.GetTagsAsync(
            query.Search,
            query.IncludeHidden,
            cancellationToken);

        return tags.ToArray();
    }
}
