using MenuMate.Common.Application;
using MenuMate.Modules.Tags.Application.Abstractions;
using MenuMate.Modules.Tags.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Tags.Application.HideTag;

internal sealed class HideTagCommandHandler(
    ITagsRepository repository,
    ITagsUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<HideTagCommand>
{
    public async Task<Result> Handle(HideTagCommand command, CancellationToken cancellationToken)
    {
        Tag? tag = await repository.GetByIdAsync(command.TagId, cancellationToken);
        if (tag is null)
        {
            return Result.Failure(TagApplicationErrors.NotFound(command.TagId));
        }

        tag.Hide(timeProvider.GetUtcNow());
        await repository.UpdateAsync(tag, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
