namespace MenuMate.Modules.RecipeImports.Infrastructure.OpenAI;

internal sealed class OpenAiRecipeCoverImageGeneratorOptions
{
    public string ApiKey { get; init; } = string.Empty;

    public string? BaseUrl { get; init; }

    public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromSeconds(150);

    public int ImageSize { get; init; } = 832;

    public string Quality { get; init; } = "medium";

    public string Model { get; init; } = "gpt-image-2";
}
