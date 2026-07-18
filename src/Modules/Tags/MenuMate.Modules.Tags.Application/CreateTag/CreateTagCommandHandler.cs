using MenuMate.Common.Application;
using MenuMate.Contracts.Tags;
using MenuMate.Modules.Tags.Application.Abstractions;
using MenuMate.Modules.Tags.Domain.Enums;
using MenuMate.Modules.Tags.Domain.Models;
using MenuMate.Modules.Tags.Domain.ValueObjects;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Tags.Application.CreateTag;

internal sealed class CreateTagCommandHandler(
    ITagsRepository repository,
    ITagsUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<CreateTagCommand, TagResponse>
{
    public async Task<Result<TagResponse>> Handle(CreateTagCommand command, CancellationToken cancellationToken)
    {
        Result<TagName> name = TagName.Create(command.Request.Name);
        if (name.IsFailure)
        {
            return Result.Failure<TagResponse>(name.Error);
        }

        if (await repository.ExistsByNormalizedNameAsync(name.Value.NormalizedValue, cancellationToken))
        {
            return Result.Failure<TagResponse>(TagApplicationErrors.DuplicateName);
        }

        var tag = Tag.Create(Guid.CreateVersion7(), name.Value, TagKind.User, timeProvider.GetUtcNow());

        await repository.AddAsync(tag, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return TagMapping.ToResponse(tag);
    }
}
