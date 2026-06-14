namespace MenuMate.Modules.RecipeImports.Infrastructure.OpenAI;

internal sealed class OpenAiRecipeImageExtractorOptions
{
    public string ApiKey { get; init; } = string.Empty;

    public string Model { get; init; } = "gpt-5.4-mini";
}
