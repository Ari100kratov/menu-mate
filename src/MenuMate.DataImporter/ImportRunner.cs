using System.Security.Cryptography;
using System.ClientModel;
using System.Text;
using System.Text.Json;
using MenuMate.Common.Application;
using MenuMate.Contracts.Recipes;
using MenuMate.DataImporter.Infrastructure.Database;
using MenuMate.DataImporter.Recipes;
using MenuMate.DataImporter.Wikibooks;
using MenuMate.Modules.Recipes.Application.CreateRecipe;
using MenuMate.Modules.Recipes.Application.UploadRecipeImage;
using MenuMate.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MenuMate.DataImporter;

internal sealed class ImportRunner(
    ImportOptions options,
    DataImportDbContext dbContext,
    WikibooksClient wikibooksClient,
    OpenAiRecipeTextExtractor openAiExtractor,
    ICommandHandler<CreateRecipeCommand, RecipeResponse> createRecipeHandler,
    ICommandHandler<UploadRecipeImageCommand, RecipeImageResponse> uploadImageHandler,
    TimeProvider timeProvider,
    ILogger<ImportRunner> logger)
{
    private static readonly Action<ILogger, long, string, Exception?> LogPageImportFailed =
        LoggerMessage.Define<long, string>(
            LogLevel.Error,
            new EventId(1, nameof(LogPageImportFailed)),
            "Не удалось импортировать страницу {PageId} ({Title}).");

    private static readonly JsonSerializerOptions ReportJsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public async Task<ImportReport> RunAsync(CancellationToken cancellationToken)
    {
        var report = new ImportReport();

        IReadOnlyCollection<WikibooksPageReference> allPages = await wikibooksClient.GetRecipePagesAsync(cancellationToken);
        if (allPages.Count == 0)
        {
            throw new InvalidOperationException(
                $"Wikibooks не вернул страницы для категории '{options.SourceCategory}'. Проверьте категорию и доступ к MediaWiki API.");
        }

        IEnumerable<WikibooksPageReference> selectedPages = options.MaxItems.HasValue
            ? allPages.Take(options.MaxItems.Value)
            : allPages;

        foreach (WikibooksPageReference reference in selectedPages)
        {
            report.PagesRead++;
            await ImportPageAsync(reference, report, cancellationToken);
        }

        await WriteReportAsync(report, cancellationToken);
        return report;
    }

    private async Task ImportPageAsync(
        WikibooksPageReference reference,
        ImportReport report,
        CancellationToken cancellationToken)
    {
        string externalId = reference.PageId.ToString(System.Globalization.CultureInfo.InvariantCulture);
        try
        {
            ImportItemRecord? state = null;
            if (!options.DryRun)
            {
                state = await dbContext.ImportItems
                    .SingleOrDefaultAsync(
                        item => item.Source == "ru.wikibooks" && item.ExternalId == externalId,
                        cancellationToken);
            }

            if (state?.Status == "Completed")
            {
                report.SkippedRecipes++;
                return;
            }

            if (state?.Status == "Failed" && !options.Resume)
            {
                report.SkippedRecipes++;
                return;
            }

            WikibooksPage page = await wikibooksClient.GetPageAsync(reference, cancellationToken);
            string contentHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(page.Wikitext)));
            CreateRecipeRequest? recipe = WikibooksRecipeParser.Parse(page);
            string? deterministicValidationError = ImportedRecipeValidator.Validate(recipe);
            string? validationError = deterministicValidationError;
            if (deterministicValidationError is not null)
            {
                recipe = await openAiExtractor.ExtractAsync(page, cancellationToken);
                report.OpenAiFallbacks++;
                validationError = ImportedRecipeValidator.Validate(recipe);
                if (validationError is not null)
                {
                    validationError =
                        $"Детерминированный парсер: {deterministicValidationError} OpenAI fallback: {validationError}";
                }
            }

            if (validationError is not null || recipe is null)
            {
                await RecordFailureAsync(state, page, contentHash, validationError ?? "Рецепт не распознан.", report, cancellationToken);
                return;
            }

            recipe = IngredientAliasNormalizer.Normalize(recipe);
            if (options.DryRun)
            {
                report.CreatedRecipes++;
                return;
            }

            state ??= new ImportItemRecord
            {
                Id = Guid.CreateVersion7(),
                Source = "ru.wikibooks",
                ExternalId = externalId,
                RecipeId = Guid.CreateVersion7()
            };
            state.SourceRevisionId = page.RevisionId;
            state.SourceUrl = page.SourceUrl;
            state.ContentHash = contentHash;
            state.Status = "Pending";
            state.LastError = null;
            state.UpdatedAt = timeProvider.GetUtcNow();
            if (dbContext.Entry(state).State == EntityState.Detached)
            {
                await dbContext.ImportItems.AddAsync(state, cancellationToken);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            Result<RecipeResponse> created = await createRecipeHandler.Handle(
                new CreateRecipeCommand(recipe, state.RecipeId),
                cancellationToken);
            if (created.IsFailure)
            {
                await RecordFailureAsync(state, page, contentHash, created.Error.Description, report, cancellationToken);
                return;
            }

            if (!options.SkipImages && page.Image is not null)
            {
                bool imported = await ImportImageAsync(created.Value, page.Image, cancellationToken);
                if (imported)
                {
                    report.ImagesImported++;
                }
                else
                {
                    report.ImagesSkipped++;
                }
            }
            else if (page.Image is not null)
            {
                report.ImagesSkipped++;
            }

            state.Status = "Completed";
            state.LastError = null;
            state.UpdatedAt = timeProvider.GetUtcNow();
            await dbContext.SaveChangesAsync(cancellationToken);
            report.CreatedRecipes++;
        }
        catch (Exception exception) when (
            exception is HttpRequestException or
                IOException or
                JsonException or
                InvalidOperationException or
                ClientResultException)
        {
            LogPageImportFailed(logger, reference.PageId, reference.Title, exception);
            report.FailedRecipes++;
            report.Failures.Add(new ImportFailure(externalId, reference.Title, exception.Message));
        }
    }

    private async Task<bool> ImportImageAsync(
        RecipeResponse recipe,
        WikibooksImage image,
        CancellationToken cancellationToken)
    {
        await using Stream source = await wikibooksClient.DownloadImageAsync(image.DownloadUrl, cancellationToken);
        await using var content = new MemoryStream();
        await source.CopyToAsync(content, cancellationToken);
        content.Position = 0;

        Result<RecipeImageResponse> result = await uploadImageHandler.Handle(
            new UploadRecipeImageCommand(
                recipe.Id,
                content,
                Path.GetFileName(image.DownloadUrl.LocalPath),
                image.ContentType,
                content.Length,
                "Cover",
                null,
                recipe.Title,
                image.SourceUrl,
                image.AuthorName,
                image.LicenseName,
                image.LicenseUrl),
            cancellationToken);
        return result.IsSuccess;
    }

    private async Task RecordFailureAsync(
        ImportItemRecord? state,
        WikibooksPage page,
        string contentHash,
        string reason,
        ImportReport report,
        CancellationToken cancellationToken)
    {
        report.FailedRecipes++;
        report.Failures.Add(new ImportFailure(page.PageId.ToString(System.Globalization.CultureInfo.InvariantCulture), page.Title, reason));
        if (options.DryRun)
        {
            return;
        }

        state ??= new ImportItemRecord
        {
            Id = Guid.CreateVersion7(),
            Source = "ru.wikibooks",
            ExternalId = page.PageId.ToString(System.Globalization.CultureInfo.InvariantCulture),
            RecipeId = Guid.CreateVersion7()
        };
        state.SourceRevisionId = page.RevisionId;
        state.SourceUrl = page.SourceUrl;
        state.ContentHash = contentHash;
        state.Status = "Failed";
        state.LastError = reason;
        state.UpdatedAt = timeProvider.GetUtcNow();
        if (dbContext.Entry(state).State == EntityState.Detached)
        {
            await dbContext.ImportItems.AddAsync(state, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task WriteReportAsync(ImportReport report, CancellationToken cancellationToken)
    {
        string json = JsonSerializer.Serialize(report, ReportJsonOptions);
        string fileName = $"data-import-report-{timeProvider.GetUtcNow():yyyyMMdd-HHmmss}.json";
        await File.WriteAllTextAsync(fileName, json, cancellationToken);
        Console.WriteLine(json);
    }
}
