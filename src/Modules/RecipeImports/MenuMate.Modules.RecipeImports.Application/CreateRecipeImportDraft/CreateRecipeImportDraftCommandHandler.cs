using MenuMate.Common.Application;
using MenuMate.Common.Application.Storage;
using MenuMate.Contracts.RecipeImports;
using MenuMate.Modules.RecipeImports.Application.Abstractions;
using MenuMate.Modules.RecipeImports.Application.Extraction;
using MenuMate.Modules.RecipeImports.Domain.Models;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.RecipeImports.Application.CreateRecipeImportDraft;

internal sealed class CreateRecipeImportDraftCommandHandler(
    IRecipeImageExtractor extractor,
    IRecipeImportDraftRepository repository,
    IRecipeImportsUnitOfWork unitOfWork,
    IObjectStorageService objectStorageService,
    RecipeImportStorageOptions storageOptions,
    RecipeImportDraftMapping mapping,
    IUserContext userContext,
    TimeProvider timeProvider)
    : ICommandHandler<CreateRecipeImportDraftCommand, RecipeImportDraftResponse>
{
    private static readonly Dictionary<string, string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp"
    };

    public async Task<Result<RecipeImportDraftResponse>> Handle(
        CreateRecipeImportDraftCommand command,
        CancellationToken cancellationToken)
    {
        Result validation = Validate(command);
        if (validation.IsFailure)
        {
            return Result.Failure<RecipeImportDraftResponse>(validation.Error);
        }

        List<SourceImageContent> images = [];
        foreach (RecipeImportSourceFile file in command.Files)
        {
            byte[] bytes = await ReadBytesAsync(file.Content, file.ContentLength, cancellationToken);
            if (!HasValidSignature(bytes, file.ContentType))
            {
                return Result.Failure<RecipeImportDraftResponse>(ImportApplicationErrors.InvalidImageSignature);
            }

            images.Add(new SourceImageContent(file, bytes));
        }

        RecipeImageExtractionResult extraction;
        try
        {
            extraction = await extractor.ExtractAsync(
                images.Select(image => new RecipeImageInput(image.Bytes, image.File.ContentType)).ToArray(),
                cancellationToken);
        }
        catch (RecipeImageExtractionException exception)
        {
            return Result.Failure<RecipeImportDraftResponse>(
                ImportApplicationErrors.ExtractionFailed(exception.Message));
        }

        var draftId = ImportDraftId.Create();
        var targetRecipeId = RecipeId.Create();
        RecipeImportSourceImage[] sourceImages =
        [
            .. images.Select((image, index) => new RecipeImportSourceImage(
                storageOptions.BucketName,
                CreateObjectKey(
                    userContext.UserId.Value,
                    draftId.Value,
                    index,
                    Extensions[image.File.ContentType]),
                image.File.ContentType,
                image.Bytes.Length,
                Path.GetFileName(image.File.FileName)))
        ];
        RecipeImportSourceImage primarySourceImage = sourceImages[0];
        DateTimeOffset now = timeProvider.GetUtcNow();
        var evidence = new RecipeImportEvidenceResponse(
            extraction.ExtractedText,
            RecipeImportWarningNormalizer.Normalize(extraction.Warnings),
            extraction.Provider,
            extraction.Model,
            extraction.ProviderResponseId);
        var draft = RecipeImportDraft.Create(
            draftId,
            userContext.UserId,
            targetRecipeId,
            extraction.Recipe.Title,
            RecipeImportJson.SerializeRecipe(extraction.Recipe),
            RecipeImportJson.SerializeEvidence(evidence),
            primarySourceImage.BucketName,
            primarySourceImage.ObjectKey,
            primarySourceImage.ContentType,
            primarySourceImage.SizeBytes,
            primarySourceImage.FileName,
            sourceImages.Skip(1).ToArray(),
            now);

        List<string> storedObjectKeys = [];
        try
        {
            await objectStorageService.EnsureBucketExistsAsync(storageOptions.BucketName, cancellationToken);
            for (int index = 0; index < images.Count; index++)
            {
                SourceImageContent image = images[index];
                RecipeImportSourceImage sourceImage = sourceImages[index];
                await objectStorageService.PutObjectAsync(
                    sourceImage.BucketName,
                    sourceImage.ObjectKey,
                    new MemoryStream(image.Bytes, writable: false),
                    image.Bytes.Length,
                    sourceImage.ContentType,
                    cancellationToken);
                storedObjectKeys.Add(sourceImage.ObjectKey);
            }

            await repository.AddAsync(draft, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ObjectStorageException exception)
        {
            await TryDeleteObjectsAsync(storedObjectKeys);
            return Result.Failure<RecipeImportDraftResponse>(
                ImportApplicationErrors.StorageFailed(exception.Message));
        }
        catch
        {
            await TryDeleteObjectsAsync(storedObjectKeys);
            throw;
        }

        return await mapping.ToResponseAsync(draft, cancellationToken);
    }

    private Result Validate(CreateRecipeImportDraftCommand command)
    {
        if (command.Files.Count == 0)
        {
            return Result.Failure(ImportApplicationErrors.ImagesRequired);
        }

        if (command.Files.Count > storageOptions.MaxImageCount)
        {
            return Result.Failure(ImportApplicationErrors.TooManyImages(storageOptions.MaxImageCount));
        }

        foreach (RecipeImportSourceFile file in command.Files)
        {
            if (file.ContentLength <= 0)
            {
                return Result.Failure(ImportApplicationErrors.EmptyImageFile);
            }

            if (file.ContentLength > storageOptions.MaxImageSizeBytes)
            {
                return Result.Failure(ImportApplicationErrors.ImageTooLarge(storageOptions.MaxImageSizeBytes));
            }

            if (!Extensions.ContainsKey(file.ContentType))
            {
                return Result.Failure(ImportApplicationErrors.UnsupportedImageContentType);
            }
        }

        if (command.Files.Sum(file => file.ContentLength) > storageOptions.MaxTotalImageSizeBytes)
        {
            return Result.Failure(
                ImportApplicationErrors.ImagesTotalSizeTooLarge(storageOptions.MaxTotalImageSizeBytes));
        }

        return Result.Success();
    }

    private static async Task<byte[]> ReadBytesAsync(
        Stream stream,
        long contentLength,
        CancellationToken cancellationToken)
    {
        using var memoryStream = new MemoryStream((int)contentLength);
        await stream.CopyToAsync(memoryStream, cancellationToken);
        return memoryStream.ToArray();
    }

    private static bool HasValidSignature(byte[] bytes, string contentType) =>
        contentType switch
        {
            "image/jpeg" => bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF,
            "image/png" => bytes.Length >= 8 &&
                bytes[0] == 0x89 &&
                bytes[1] == 0x50 &&
                bytes[2] == 0x4E &&
                bytes[3] == 0x47 &&
                bytes[4] == 0x0D &&
                bytes[5] == 0x0A &&
                bytes[6] == 0x1A &&
                bytes[7] == 0x0A,
            "image/webp" => bytes.Length >= 12 &&
                bytes.AsSpan(0, 4).SequenceEqual("RIFF"u8) &&
                bytes.AsSpan(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        };

    private static string CreateObjectKey(Guid ownerUserId, Guid draftId, int index, string extension) =>
        $"users/{ownerUserId:N}/imports/{draftId:N}/source-{index + 1}{extension}";

    private async Task TryDeleteObjectsAsync(IEnumerable<string> objectKeys)
    {
        foreach (string objectKey in objectKeys)
        {
            try
            {
                await objectStorageService.DeleteObjectAsync(
                    storageOptions.BucketName,
                    objectKey,
                    CancellationToken.None);
            }
            catch (ObjectStorageException)
            {
                // Очистка является best-effort после уже произошедшего сбоя.
            }
        }
    }

    private sealed record SourceImageContent(RecipeImportSourceFile File, byte[] Bytes);
}
