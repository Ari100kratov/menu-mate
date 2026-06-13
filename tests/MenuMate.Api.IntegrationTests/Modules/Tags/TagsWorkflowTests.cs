using System.Net;
using System.Net.Http.Json;
using MenuMate.Contracts.Tags;

namespace MenuMate.Api.IntegrationTests;

public sealed class TagsWorkflowTests : IAsyncLifetime, IDisposable
{
    private readonly MenuMateApiFactory _factory = new();

    public Task InitializeAsync() => _factory.InitializeAsync();

    public Task DisposeAsync() => _factory.DisposeAsync();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task SuggestedTagShouldSupportCreateSearchConfirmAndHide()
    {
        using HttpClient httpClient = _factory.CreateClient();
        var client = new ApiTestClient(httpClient);
        await client.RegisterAsync(TestEmail.Create("tags"));

        HttpResponseMessage createResponse = await httpClient.PostAsJsonAsync(
            "/api/tags/",
            new CreateTagRequest("  Быстрый ужин  ", "Suggested"));
        createResponse.EnsureSuccessStatusCode();
        TagResponse? created = await createResponse.Content.ReadFromJsonAsync<TagResponse>();
        Assert.NotNull(created);
        Assert.Equal("Быстрый ужин", created.Name);
        Assert.Equal("БЫСТРЫЙ УЖИН", created.NormalizedName);
        Assert.Equal("Proposed", created.Status);

        TagResponse[]? search = await httpClient.GetFromJsonAsync<TagResponse[]>("/api/tags?search=ужин");
        Assert.NotNull(search);
        Assert.Equal(created.Id, Assert.Single(search).Id);

        HttpResponseMessage confirmResponse = await httpClient.PostAsync(
            RelativeUri($"/api/tags/{created.Id}/confirm"),
            content: null);
        confirmResponse.EnsureSuccessStatusCode();

        HttpResponseMessage hideResponse = await httpClient.DeleteAsync(
            RelativeUri($"/api/tags/{created.Id}"));
        hideResponse.EnsureSuccessStatusCode();

        Assert.Empty(await GetTagsAsync(httpClient, includeHidden: false));
        TagResponse hidden = Assert.Single(await GetTagsAsync(httpClient, includeHidden: true));
        Assert.Equal("Hidden", hidden.Status);
    }

    [Fact]
    public async Task DuplicateNormalizedTagNameShouldReturnConflict()
    {
        using HttpClient httpClient = _factory.CreateClient();
        var client = new ApiTestClient(httpClient);
        await client.RegisterAsync(TestEmail.Create("tag-duplicate"));

        HttpResponseMessage first = await httpClient.PostAsJsonAsync(
            "/api/tags/",
            new CreateTagRequest("Быстро", "User"));
        first.EnsureSuccessStatusCode();

        HttpResponseMessage duplicate = await httpClient.PostAsJsonAsync(
            "/api/tags/",
            new CreateTagRequest("  быстро ", "User"));

        await ProblemDetailsAssert.HasProblemAsync(
            duplicate,
            HttpStatusCode.Conflict,
            "Tags.DuplicateName");
    }

    private static async Task<TagResponse[]> GetTagsAsync(HttpClient client, bool includeHidden)
    {
        TagResponse[]? tags = await client.GetFromJsonAsync<TagResponse[]>(
            $"/api/tags?includeHidden={includeHidden}");
        Assert.NotNull(tags);
        return tags;
    }

    private static Uri RelativeUri(string path) => new(path, UriKind.Relative);
}
