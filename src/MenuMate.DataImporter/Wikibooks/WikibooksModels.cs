namespace MenuMate.DataImporter.Wikibooks;

internal sealed record WikibooksPageReference(long PageId, string Title);

internal sealed record WikibooksPage(
    long PageId,
    long RevisionId,
    string Title,
    string Wikitext,
    Uri SourceUrl,
    WikibooksImage? Image);

internal sealed record WikibooksImage(
    Uri DownloadUrl,
    Uri SourceUrl,
    string? AuthorName,
    string LicenseName,
    Uri? LicenseUrl,
    string ContentType);
