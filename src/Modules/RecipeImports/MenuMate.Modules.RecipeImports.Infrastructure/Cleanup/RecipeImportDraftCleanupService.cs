using MenuMate.Common.Application.Storage;
using MenuMate.Modules.RecipeImports.Application;
using MenuMate.Modules.RecipeImports.Application.Abstractions;
using MenuMate.Modules.RecipeImports.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MenuMate.Modules.RecipeImports.Infrastructure.Cleanup;

internal sealed class RecipeImportDraftCleanupService(
    IServiceScopeFactory scopeFactory,
    RecipeImportStorageOptions options,
    TimeProvider timeProvider,
    ILogger<RecipeImportDraftCleanupService> logger)
    : BackgroundService
{
    private static readonly Action<ILogger, Exception> LogCleanupFailure = LoggerMessage.Define(
        LogLevel.Error,
        new EventId(1, nameof(LogCleanupFailure)),
        "Не удалось удалить просроченные черновики импорта рецептов.");

    private static readonly Action<ILogger, string, Exception> LogSourceImageDeleteFailure =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(2, nameof(LogSourceImageDeleteFailure)),
            "Не удалось удалить исходное изображение просроченного черновика {ObjectKey}.");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DeleteExpiredDraftsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
#pragma warning disable CA1031 // Фоновая очистка должна продолжить работу после сбоя одной итерации.
            catch (Exception exception)
            {
                LogCleanupFailure(logger, exception);
            }
#pragma warning restore CA1031

            await Task.Delay(options.CleanupInterval, timeProvider, stoppingToken);
        }
    }

    private async Task DeleteExpiredDraftsAsync(CancellationToken cancellationToken)
    {
        bool hasMore;
        do
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
            IRecipeImportDraftRepository repository =
                scope.ServiceProvider.GetRequiredService<IRecipeImportDraftRepository>();
            IRecipeImportsUnitOfWork unitOfWork =
                scope.ServiceProvider.GetRequiredService<IRecipeImportsUnitOfWork>();
            IObjectStorageService objectStorageService =
                scope.ServiceProvider.GetRequiredService<IObjectStorageService>();

            DateTimeOffset cutoff = timeProvider.GetUtcNow() - options.DraftRetentionPeriod;
            IReadOnlyCollection<RecipeImportDraft> drafts = await repository.GetExpiredAsync(
                cutoff,
                options.CleanupBatchSize,
                cancellationToken);

            foreach (RecipeImportDraft draft in drafts)
            {
                await repository.DeleteAsync(draft, cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (RecipeImportSourceImage sourceImage in drafts.SelectMany(draft => draft.SourceImages))
            {
                try
                {
                    await objectStorageService.DeleteObjectAsync(
                        sourceImage.BucketName,
                        sourceImage.ObjectKey,
                        cancellationToken);
                }
                catch (ObjectStorageException exception)
                {
                    LogSourceImageDeleteFailure(logger, sourceImage.ObjectKey, exception);
                }
            }

            hasMore = drafts.Count == options.CleanupBatchSize;
        }
        while (hasMore);
    }
}
