namespace MenuMate.Modules.RecipeImports.Infrastructure.OpenAI;

internal sealed class OpenAiRecipeCoverImageGeneratorOptions
{
    public string ApiKey { get; init; } = string.Empty;

    public string? BaseUrl { get; init; }

    public string Model { get; init; } = "gpt-image-1-mini";
}
