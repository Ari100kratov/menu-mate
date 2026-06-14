#pragma warning disable OPENAI001 // JPEG и сжатие помечены experimental в официальном SDK OpenAI 2.11.0.

using System.ClientModel;
using System.Text.Json;
using MenuMate.Contracts.Recipes;
using MenuMate.Modules.RecipeImports.Application.Generation;
using Microsoft.Extensions.Logging;
using OpenAI.Images;

namespace MenuMate.Modules.RecipeImports.Infrastructure.OpenAI;

internal sealed class OpenAiRecipeCoverImageGenerator(
    ImageClient client,
    OpenAiRecipeCoverImageGeneratorOptions options,
    ILogger<OpenAiRecipeCoverImageGenerator> logger)
    : IRecipeCoverImageGenerator
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly Action<ILogger, string, int, Exception> LogOpenAiFailure =
        LoggerMessage.Define<string, int>(
            LogLevel.Error,
            new EventId(1, nameof(LogOpenAiFailure)),
            "OpenAI Image API rejected cover generation with model {Model} and HTTP status {StatusCode}.");

    private static readonly Action<ILogger, string, Exception> LogUnexpectedFailure =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(2, nameof(LogUnexpectedFailure)),
            "Unexpected cover generation failure with OpenAI image model {Model}.");

    public async Task<GeneratedRecipeCoverImage> GenerateAsync(
        CreateRecipeRequest recipe,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey) ||
            string.Equals(options.ApiKey, "not-configured", StringComparison.Ordinal))
        {
            throw new RecipeCoverImageGenerationException("Серверный ключ OpenAI не настроен.");
        }

        try
        {
            string prompt =
                """
                Создай реалистичную аппетитную фотографию готового блюда для обложки рецепта.
                Покажи только само блюдо в аккуратной естественной сервировке, без текста,
                логотипов, водяных знаков, людей и процесса приготовления.
                Используй все данные рецепта ниже, чтобы изображение соответствовало ингредиентам
                и итоговому результату. Квадратная композиция, мягкий естественный свет.

                Рецепт:
                """ +
                JsonSerializer.Serialize(recipe, JsonOptions);

            GeneratedImage image = await client.GenerateImageAsync(
                prompt,
                new ImageGenerationOptions
                {
                    Quality = new GeneratedImageQuality("low"),
                    Size = GeneratedImageSize.W1024xH1024,
                    OutputFileFormat = GeneratedImageFileFormat.Jpeg,
                    OutputCompressionFactor = 75
                },
                cancellationToken);

            ReadOnlyMemory<byte> imageBytes = image.ImageBytes.ToMemory();
            if (imageBytes.IsEmpty)
            {
                throw new RecipeCoverImageGenerationException(
                    "OpenAI вернул пустое изображение.");
            }

            return new GeneratedRecipeCoverImage(
                imageBytes,
                "image/jpeg",
                "ai-recipe-cover.jpg");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (RecipeCoverImageGenerationException)
        {
            throw;
        }
        catch (ClientResultException exception)
        {
            LogOpenAiFailure(logger, options.Model, exception.Status, exception);
            throw new RecipeCoverImageGenerationException(
                CreateProviderErrorMessage(exception),
                exception);
        }
        catch (Exception exception)
        {
            LogUnexpectedFailure(logger, options.Model, exception);
            throw new RecipeCoverImageGenerationException(
                "Внешний сервис генерации изображений временно недоступен.",
                exception);
        }
    }

    private static string CreateProviderErrorMessage(ClientResultException exception)
    {
        if (ContainsErrorCode(exception, "billing_hard_limit_reached") ||
            ContainsErrorCode(exception, "billing_limit_user_error") ||
            ContainsErrorCode(exception, "insufficient_quota"))
        {
            return "Для проекта OpenAI исчерпан баланс или достигнут жёсткий лимит расходов. Пополните баланс или увеличьте бюджет API.";
        }

        return exception.Status switch
        {
            400 => "OpenAI отклонил параметры запроса (HTTP 400). Подробности записаны в журнал сервера.",
            401 => "Ключ OpenAI недействителен или не имеет нужных прав (HTTP 401).",
            403 => "У проекта OpenAI нет доступа к модели генерации изображений. Проверьте права ключа и верификацию организации (HTTP 403).",
            429 => "OpenAI отклонил запрос из-за лимита или отсутствия доступной квоты (HTTP 429).",
            >= 500 => $"OpenAI временно недоступен (HTTP {exception.Status}).",
            > 0 => $"OpenAI отклонил запрос (HTTP {exception.Status}). Подробности записаны в журнал сервера.",
            _ => "Не удалось подключиться к OpenAI Image API."
        };
    }

    private static bool ContainsErrorCode(ClientResultException exception, string errorCode) =>
        exception.Message.Contains(errorCode, StringComparison.OrdinalIgnoreCase);
}
