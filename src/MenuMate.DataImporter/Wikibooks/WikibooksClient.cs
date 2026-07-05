using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MenuMate.DataImporter.Wikibooks;

internal sealed partial class WikibooksClient(HttpClient httpClient, ImportOptions options)
{
    public async Task<IReadOnlyCollection<WikibooksPageReference>> GetRecipePagesAsync(
        CancellationToken cancellationToken)
    {
        var result = new List<WikibooksPageReference>();
        string? continuation = null;

        do
        {
            string url = $"{options.ApiUrl}?action=query&format=json&formatversion=2&list=categorymembers" +
                $"&cmtitle={Uri.EscapeDataString($"Категория:{options.SourceCategory}")}&cmtype=page&cmlimit=max";
            if (!string.IsNullOrWhiteSpace(continuation))
            {
                url += $"&cmcontinue={Uri.EscapeDataString(continuation)}";
            }

            using JsonDocument document = await GetJsonAsync(url, cancellationToken);
            foreach (JsonElement member in document.RootElement.GetProperty("query").GetProperty("categorymembers").EnumerateArray())
            {
                result.Add(new WikibooksPageReference(
                    member.GetProperty("pageid").GetInt64(),
                    member.GetProperty("title").GetString() ?? string.Empty));
            }

            continuation = document.RootElement.TryGetProperty("continue", out JsonElement continueElement)
                ? continueElement.GetProperty("cmcontinue").GetString()
                : null;
        }
        while (!string.IsNullOrWhiteSpace(continuation));

        return result;
    }

    public async Task<WikibooksPage> GetPageAsync(
        WikibooksPageReference reference,
        CancellationToken cancellationToken)
    {
        string url = $"{options.ApiUrl}?action=query&format=json&formatversion=2&pageids={reference.PageId}" +
            "&prop=revisions%7Cpageimages&rvprop=ids%7Ccontent&rvslots=main&piprop=name";
        using JsonDocument document = await GetJsonAsync(url, cancellationToken);
        JsonElement page = document.RootElement.GetProperty("query").GetProperty("pages")[0];
        JsonElement revision = page.GetProperty("revisions")[0];
        string wikitext = revision.GetProperty("slots").GetProperty("main").GetProperty("content").GetString() ?? string.Empty;
        WikibooksImage? image = page.TryGetProperty("pageimage", out JsonElement pageImage)
            ? await GetImageAsync(pageImage.GetString() ?? string.Empty, cancellationToken)
            : null;

        return new WikibooksPage(
            reference.PageId,
            revision.GetProperty("revid").GetInt64(),
            reference.Title,
            wikitext,
            new Uri(options.SiteUrl, Uri.EscapeDataString(reference.Title.Replace(' ', '_'))),
            image);
    }

    public async Task<Stream> DownloadImageAsync(Uri url, CancellationToken cancellationToken) =>
        await httpClient.GetStreamAsync(url, cancellationToken);

    private async Task<WikibooksImage?> GetImageAsync(string title, CancellationToken cancellationToken)
    {
        string url = $"{options.ApiUrl}?action=query&format=json&formatversion=2&titles={Uri.EscapeDataString($"Файл:{title}")}" +
            "&prop=imageinfo&iiprop=url%7Cmime%7Cextmetadata&iiurlwidth=1200";
        using JsonDocument document = await GetJsonAsync(url, cancellationToken);
        JsonElement page = document.RootElement.GetProperty("query").GetProperty("pages")[0];
        if (!page.TryGetProperty("imageinfo", out JsonElement imageInfoArray))
        {
            return null;
        }

        JsonElement imageInfo = imageInfoArray[0];
        JsonElement metadata = imageInfo.GetProperty("extmetadata");
        string licenseName = ReadMetadata(metadata, "LicenseShortName") ?? string.Empty;
        if (!ImageLicensePolicy.IsAllowed(licenseName))
        {
            return null;
        }

        string? sourceUrl = imageInfo.GetProperty("descriptionurl").GetString();
        string? downloadUrl = imageInfo.TryGetProperty("thumburl", out JsonElement thumbnail)
            ? thumbnail.GetString()
            : imageInfo.GetProperty("url").GetString();
        if (!Uri.TryCreate(sourceUrl, UriKind.Absolute, out Uri? sourceUri) ||
            !Uri.TryCreate(downloadUrl, UriKind.Absolute, out Uri? downloadUri))
        {
            return null;
        }

        return new WikibooksImage(
            downloadUri,
            sourceUri,
            StripMarkup(ReadMetadata(metadata, "Artist")),
            licenseName,
            Uri.TryCreate(ReadMetadata(metadata, "LicenseUrl"), UriKind.Absolute, out Uri? licenseUri)
                ? licenseUri
                : null,
            imageInfo.GetProperty("mime").GetString() ?? "image/jpeg");
    }

    private async Task<JsonDocument> GetJsonAsync(string url, CancellationToken cancellationToken)
    {
        if (options.RequestDelayMilliseconds > 0)
        {
            await Task.Delay(options.RequestDelayMilliseconds, cancellationToken);
        }

        JsonDocument? document = await httpClient.GetFromJsonAsync<JsonDocument>(url, cancellationToken);
        return document ?? throw new InvalidOperationException("Wikibooks вернул пустой ответ.");
    }

    private static string? ReadMetadata(JsonElement metadata, string name) =>
        metadata.TryGetProperty(name, out JsonElement value) &&
        value.TryGetProperty("value", out JsonElement rawValue)
            ? rawValue.GetString()
            : null;

    private static string? StripMarkup(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string result = HtmlTagRegex().Replace(value, string.Empty).Trim();
        return result.Length <= 500 ? result : result[..500];
    }

    [GeneratedRegex("<[^>]+>", RegexOptions.CultureInvariant)]
    private static partial Regex HtmlTagRegex();
}
