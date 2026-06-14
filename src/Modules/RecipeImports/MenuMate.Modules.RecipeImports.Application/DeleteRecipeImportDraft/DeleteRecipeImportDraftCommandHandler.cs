using MenuMate.Common.Application;
using MenuMate.Common.Application.Storage;
using MenuMate.Modules.RecipeImports.Application.Abstractions;
using MenuMate.Modules.RecipeImports.Domain.Models;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.RecipeImports.Application.DeleteRecipeImportDraft;

internal sealed class DeleteRecipeImportDraftCommandHandler(
    IRecipeImportDraftRepository repository,
    IRecipeImportsUnitOfWork unitOfWork,
    IObjectStorageService objectStorageService,
    IUserContext userContext)
    : ICommandHandler<DeleteRecipeImportDraftCommand>
{
    public async Task<Result> Handle(
        DeleteRecipeImportDraftCommand command,
        CancellationToken cancellationToken)
    {
        RecipeImportDraft? draft = await repository.GetByIdAsync(
            ImportDraftId.From(command.DraftId),
            cancellationToken);
        if (draft is null)
        {
            return Result.Failure(ImportApplicationErrors.NotFound(command.DraftId));
        }

        if (draft.OwnerUserId != userContext.UserId)
        {
            return Result.Failure(ImportApplicationErrors.AccessDenied);
        }

        await repository.DeleteAsync(draft, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (RecipeImportSourceImage sourceImage in draft.SourceImages)
        {
            try
            {
                await objectStorageService.DeleteObjectAsync(
                    sourceImage.BucketName,
                    sourceImage.ObjectKey,
                    cancellationToken);
            }
            catch (ObjectStorageException)
            {
                // Черновик уже удален; очистка объекта является best-effort.
            }
        }

        return Result.Success();
    }
}
