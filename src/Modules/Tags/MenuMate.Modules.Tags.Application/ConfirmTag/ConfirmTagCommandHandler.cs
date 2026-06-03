using MenuMate.Common.Application;
using MenuMate.Modules.Tags.Application.Abstractions;
using MenuMate.Modules.Tags.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Tags.Application.ConfirmTag;

internal sealed class ConfirmTagCommandHandler(
    ITagsRepository repository,
    ITagsUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<ConfirmTagCommand>
{
    public async Task<Result> Handle(ConfirmTagCommand command, CancellationToken cancellationToken)
    {
        Tag? tag = await repository.GetByIdAsync(command.TagId, cancellationToken);
        if (tag is null)
        {
            return Result.Failure(TagApplicationErrors.NotFound(command.TagId));
        }

        tag.Confirm(timeProvider.GetUtcNow());
        await repository.UpdateAsync(tag, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
