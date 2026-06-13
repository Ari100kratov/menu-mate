using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace MenuMate.Api.IntegrationTests;

internal static class ProblemDetailsAssert
{
    public static async Task HasProblemAsync(
        HttpResponseMessage response,
        HttpStatusCode statusCode,
        string title)
    {
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        JsonElement problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal((int)statusCode, problem.GetProperty("status").GetInt32());
        Assert.Equal(title, problem.GetProperty("title").GetString());
    }
}
