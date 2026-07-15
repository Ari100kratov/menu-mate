using MenuMate.Common.Application;
using MenuMate.Common.Application.Storage;
using MenuMate.Modules.RecipeImports.Application.Abstractions;
using MenuMate.Modules.RecipeImports.Domain.Models;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.RecipeImports.Application.GetRecipeImportSourceImage;

internal sealed class GetRecipeImportSourceImageQueryHandler(
    IRecipeImportDraftRepository repository,
    IObjectStorageService objectStorageService,
    IUserContext userContext)
    : IQueryHandler<GetRecipeImportSourceImageQuery, RecipeImportSourceImageContent>
{
    public async Task<Result<RecipeImportSourceImageContent>> Handle(
        GetRecipeImportSourceImageQuery query,
        CancellationToken cancellationToken)
    {
        RecipeImportDraft? draft = await repository.GetByIdAsync(
            ImportDraftId.From(query.DraftId),
            cancellationToken);
        if (draft is null)
        {
            return Result.Failure<RecipeImportSourceImageContent>(
                ImportApplicationErrors.NotFound(query.DraftId));
        }

        if (draft.OwnerUserId != userContext.UserId)
        {
            return Result.Failure<RecipeImportSourceImageContent>(ImportApplicationErrors.AccessDenied);
        }

        RecipeImportSourceImage? sourceImage = query.SourceImageIndex < 0
            ? null
            : draft.SourceImages.ElementAtOrDefault(query.SourceImageIndex);
        if (sourceImage is null)
        {
            return Result.Failure<RecipeImportSourceImageContent>(
                ImportApplicationErrors.SourceImageNotFound(
                    query.DraftId,
                    query.SourceImageIndex));
        }

        try
        {
            Stream content = await objectStorageService.GetObjectStreamAsync(
                sourceImage.BucketName,
                sourceImage.ObjectKey,
                cancellationToken);
            return new RecipeImportSourceImageContent(
                content,
                sourceImage.ContentType,
                sourceImage.FileName);
        }
        catch (ObjectStorageException exception)
        {
            return Result.Failure<RecipeImportSourceImageContent>(
                ImportApplicationErrors.SourceImageReadFailed(exception.Message));
        }
    }
}
