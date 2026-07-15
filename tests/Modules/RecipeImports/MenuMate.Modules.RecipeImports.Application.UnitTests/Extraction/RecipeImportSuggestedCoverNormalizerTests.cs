using MenuMate.Contracts.RecipeImports;
using MenuMate.Modules.RecipeImports.Application.Extraction;

namespace MenuMate.Modules.RecipeImports.Application.UnitTests.Extraction;

public sealed class RecipeImportSuggestedCoverNormalizerTests
{
    [Fact]
    public void NormalizeShouldKeepValidCandidate()
    {
        var candidate = new RecipeImportSuggestedCoverResponse(
            SourceImageIndex: 1,
            X: 100,
            Y: 150,
            Width: 700,
            Height: 600,
            Confidence: 70);

        RecipeImportSuggestedCoverResponse? result =
            RecipeImportSuggestedCoverNormalizer.Normalize(candidate, sourceImageCount: 2);

        Assert.Equal(candidate, result);
    }

    [Theory]
    [InlineData(0, 0, 0, 1000, 1000, 69, 1)]
    [InlineData(1, 0, 0, 1000, 1000, 100, 1)]
    [InlineData(0, -1, 0, 100, 100, 100, 1)]
    [InlineData(0, 0, -1, 100, 100, 100, 1)]
    [InlineData(0, 900, 0, 101, 100, 100, 1)]
    [InlineData(0, 0, 900, 100, 101, 100, 1)]
    [InlineData(0, 0, 0, 0, 100, 100, 1)]
    [InlineData(0, 0, 0, 100, 0, 100, 1)]
    [InlineData(0, 0, 0, 100, 100, 101, 1)]
    public void NormalizeShouldDiscardInvalidCandidate(
        int sourceImageIndex,
        int x,
        int y,
        int width,
        int height,
        int confidence,
        int sourceImageCount)
    {
        var candidate = new RecipeImportSuggestedCoverResponse(
            sourceImageIndex,
            x,
            y,
            width,
            height,
            confidence);

        RecipeImportSuggestedCoverResponse? result =
            RecipeImportSuggestedCoverNormalizer.Normalize(candidate, sourceImageCount);

        Assert.Null(result);
    }

    [Fact]
    public void DeserializeEvidenceShouldSupportDraftsWithoutSuggestedCover()
    {
        const string Json =
            """
            {
              "extractedText": "Рецепт",
              "warnings": [],
              "provider": "OpenAI",
              "model": "test-model",
              "providerResponseId": null
            }
            """;

        RecipeImportEvidenceResponse evidence = RecipeImportJson.DeserializeEvidence(Json);

        Assert.Null(evidence.SuggestedCover);
    }
}
