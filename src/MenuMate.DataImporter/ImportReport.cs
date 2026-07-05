namespace MenuMate.DataImporter;

internal sealed class ImportReport
{
    public int PagesRead { get; set; }

    public int CreatedRecipes { get; set; }

    public int SkippedRecipes { get; set; }

    public int FailedRecipes { get; set; }

    public int ImagesImported { get; set; }

    public int ImagesSkipped { get; set; }

    public int OpenAiFallbacks { get; set; }

    public List<ImportFailure> Failures { get; } = [];
}

internal sealed record ImportFailure(string ExternalId, string Title, string Reason);
