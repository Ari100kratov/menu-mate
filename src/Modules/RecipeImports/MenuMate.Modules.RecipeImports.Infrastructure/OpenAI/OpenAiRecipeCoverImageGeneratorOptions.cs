namespace MenuMate.Modules.RecipeImports.Infrastructure.OpenAI;

internal sealed class OpenAiRecipeCoverImageGeneratorOptions
{
    public string ApiKey { get; init; } = string.Empty;

    public string Model { get; init; } = "gpt-image-1-mini";
}
