using System.Net;
using System.Text;
using MenuMate.DataImporter.Wikibooks;

namespace MenuMate.DataImporter.UnitTests;

public sealed class WikibooksClientTests
{
    [Fact]
    public async Task GetRecipePagesAsyncReadsPagesFromRecipeNamespace()
    {
        using var handler = new RecordingHttpMessageHandler(
            """
            {
              "query": {
                "categorymembers": [
                  { "pageid": 21569, "ns": 104, "title": "Рецепт:Авголемоно" }
                ]
              }
            }
            """);
        using var httpClient = new HttpClient(handler);
        var options = new ImportOptions(
            string.Empty,
            null,
            null,
            true,
            false,
            true,
            "Рецепты",
            "MenuMateDataImporter.Tests",
            0,
            new Uri("https://ru.wikibooks.org/w/api.php"),
            new Uri("https://ru.wikibooks.org/wiki/"));
        var client = new WikibooksClient(httpClient, options);

        IReadOnlyCollection<WikibooksPageReference> pages =
            await client.GetRecipePagesAsync(CancellationToken.None);

        WikibooksPageReference page = Assert.Single(pages);
        Assert.Equal(21569, page.PageId);
        Assert.Equal("Рецепт:Авголемоно", page.Title);
        Assert.NotNull(handler.RequestUri);
        Assert.Contains("cmtype=page", handler.RequestUri.Query, StringComparison.Ordinal);
        Assert.DoesNotContain("cmnamespace=0", handler.RequestUri.Query, StringComparison.Ordinal);
    }

    private sealed class RecordingHttpMessageHandler(string responseBody) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            RequestUri = request.RequestUri;
            return Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
                });
        }
    }
}
