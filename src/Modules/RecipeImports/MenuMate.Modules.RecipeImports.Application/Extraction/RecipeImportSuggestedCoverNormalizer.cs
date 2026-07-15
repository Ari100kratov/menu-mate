using MenuMate.Contracts.RecipeImports;

namespace MenuMate.Modules.RecipeImports.Application.Extraction;

internal static class RecipeImportSuggestedCoverNormalizer
{
    private const int CoordinateScale = 1000;
    private const int MinimumConfidence = 70;

    public static RecipeImportSuggestedCoverResponse? Normalize(
        RecipeImportSuggestedCoverResponse? candidate,
        int sourceImageCount)
    {
        if (candidate is null ||
            candidate.Confidence < MinimumConfidence ||
            candidate.Confidence > 100 ||
            candidate.SourceImageIndex < 0 ||
            candidate.SourceImageIndex >= sourceImageCount ||
            candidate.X < 0 ||
            candidate.X > CoordinateScale ||
            candidate.Y < 0 ||
            candidate.Y > CoordinateScale ||
            candidate.Width <= 0 ||
            candidate.Width > CoordinateScale ||
            candidate.Height <= 0 ||
            candidate.Height > CoordinateScale ||
            candidate.X + candidate.Width > CoordinateScale ||
            candidate.Y + candidate.Height > CoordinateScale)
        {
            return null;
        }

        return candidate;
    }
}
