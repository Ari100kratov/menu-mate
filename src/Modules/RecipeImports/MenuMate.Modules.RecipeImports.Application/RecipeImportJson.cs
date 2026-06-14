using System.Text.Json;
using MenuMate.Contracts.RecipeImports;
using MenuMate.Contracts.Recipes;

namespace MenuMate.Modules.RecipeImports.Application;

internal static class RecipeImportJson
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static string SerializeRecipe(CreateRecipeRequest recipe) =>
        JsonSerializer.Serialize(recipe, Options);

    public static CreateRecipeRequest DeserializeRecipe(string json) =>
        JsonSerializer.Deserialize<CreateRecipeRequest>(json, Options)
        ?? throw new InvalidOperationException("Снимок рецепта в черновике импорта пуст.");

    public static string SerializeEvidence(RecipeImportEvidenceResponse evidence) =>
        JsonSerializer.Serialize(evidence, Options);

    public static RecipeImportEvidenceResponse DeserializeEvidence(string json) =>
        JsonSerializer.Deserialize<RecipeImportEvidenceResponse>(json, Options)
        ?? throw new InvalidOperationException("Доказательства распознавания в черновике импорта пусты.");
}
