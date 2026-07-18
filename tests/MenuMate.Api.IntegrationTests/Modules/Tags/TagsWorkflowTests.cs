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
    public async Task UserTagShouldBeCreatedAsConfirmedAndFoundBySearch()
    {
        using HttpClient httpClient = _factory.CreateClient();
        var client = new ApiTestClient(httpClient);
        await client.RegisterAsync(TestEmail.Create("tags"));

        HttpResponseMessage createResponse = await httpClient.PostAsJsonAsync(
            "/api/tags/",
            new CreateTagRequest("  Быстрый ужин  "));
        createResponse.EnsureSuccessStatusCode();
        TagResponse? created = await createResponse.Content.ReadFromJsonAsync<TagResponse>();
        Assert.NotNull(created);
        Assert.Equal("Быстрый ужин", created.Name);
        Assert.Equal("БЫСТРЫЙ УЖИН", created.NormalizedName);
        Assert.Equal("User", created.Kind);
        Assert.Equal("Confirmed", created.Status);

        TagResponse[]? search = await httpClient.GetFromJsonAsync<TagResponse[]>("/api/tags?search=ужин");
        Assert.NotNull(search);
        Assert.Equal(created.Id, Assert.Single(search).Id);

        Assert.Equal(created.Id, Assert.Single(await GetTagsAsync(httpClient)).Id);
    }

    [Fact]
    public async Task DuplicateNormalizedTagNameShouldReturnConflict()
    {
        using HttpClient httpClient = _factory.CreateClient();
        var client = new ApiTestClient(httpClient);
        await client.RegisterAsync(TestEmail.Create("tag-duplicate"));

        HttpResponseMessage first = await httpClient.PostAsJsonAsync(
            "/api/tags/",
            new CreateTagRequest("Быстро"));
        first.EnsureSuccessStatusCode();

        HttpResponseMessage duplicate = await httpClient.PostAsJsonAsync(
            "/api/tags/",
            new CreateTagRequest("  быстро "));

        await ProblemDetailsAssert.HasProblemAsync(
            duplicate,
            HttpStatusCode.Conflict,
            "Tags.DuplicateName");
    }

    [Fact]
    public async Task SearchShouldRankExactThenPrefixThenWholeWordThenSubstring()
    {
        using HttpClient httpClient = _factory.CreateClient();
        var client = new ApiTestClient(httpClient);
        await client.RegisterAsync(TestEmail.Create("tag-ranking"));

        string[] names = ["Малосольные огурцы", "Морская соль", "Соль крупная", "Соль"];
        foreach (string name in names)
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync(
                "/api/tags/",
                new CreateTagRequest(name));
            response.EnsureSuccessStatusCode();
        }

        TagResponse[]? results = await httpClient.GetFromJsonAsync<TagResponse[]>(
            "/api/tags?search=соль");

        Assert.NotNull(results);
        Assert.Equal(
            ["Соль", "Соль крупная", "Морская соль", "Малосольные огурцы"],
            results.Select(tag => tag.Name));
    }

    private static async Task<TagResponse[]> GetTagsAsync(HttpClient client)
    {
        TagResponse[]? tags = await client.GetFromJsonAsync<TagResponse[]>("/api/tags");
        Assert.NotNull(tags);
        return tags;
    }
}
