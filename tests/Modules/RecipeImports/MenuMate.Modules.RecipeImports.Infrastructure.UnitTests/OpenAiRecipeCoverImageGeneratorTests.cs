using System.ClientModel;
using System.ClientModel.Primitives;
using System.Net;
using System.Text;
using System.Text.Json;
using MenuMate.Contracts.Recipes;
using MenuMate.Modules.RecipeImports.Application.Generation;
using MenuMate.Modules.RecipeImports.Infrastructure.OpenAI;
using Microsoft.Extensions.Logging.Abstractions;
using OpenAI;
using OpenAI.Images;

namespace MenuMate.Modules.RecipeImports.Infrastructure.UnitTests;

public sealed class OpenAiRecipeCoverImageGeneratorTests
{
    [Fact]
    public async Task GenerateAsyncSendsCompleteRecipeAndHighQualityOptionsReturnsJpeg()
    {
        CreateRecipeRequest recipe = new(
            "Тыквенный суп", "Густой крем-суп", 2, "Soup", "Private", 40, 15, null,
            [new RecipeIngredientRequest(null, "Тыква", 500, "Gram", "Vegetables", "Очистить", false)],
            [new PreparationStepRequest("Запечь тыкву."), new PreparationStepRequest("Измельчить в пюре.")],
            ["осенний"], "Подавать горячим.");
        using RecordingHandler handler = new();
        using HttpClient httpClient = new(handler);
        OpenAiRecipeCoverImageGeneratorOptions options = new() { ApiKey = "test-key" };
        ImageClient client = new(options.Model, new ApiKeyCredential(options.ApiKey),
            new OpenAIClientOptions { Transport = new HttpClientPipelineTransport(httpClient) });
        OpenAiRecipeCoverImageGenerator generator = new(client, options,
            NullLogger<OpenAiRecipeCoverImageGenerator>.Instance);

        GeneratedRecipeCoverImage result = await generator.GenerateAsync(recipe, CancellationToken.None);

        Assert.Equal("image/jpeg", result.ContentType);
        Assert.Equal(new byte[] { 1, 2, 3 }, result.Content.ToArray());
        using var request = JsonDocument.Parse(Assert.IsType<string>(handler.RequestBody));
        JsonElement root = request.RootElement;
        Assert.Equal("gpt-image-2", root.GetProperty("model").GetString());
        Assert.Equal("high", root.GetProperty("quality").GetString());
        Assert.Equal("1024x1024", root.GetProperty("size").GetString());
        Assert.Equal("jpeg", root.GetProperty("output_format").GetString());
        Assert.Equal(90, root.GetProperty("output_compression").GetInt32());
        string prompt = Assert.IsType<string>(root.GetProperty("prompt").GetString());
        using var payload = JsonDocument.Parse(prompt[prompt.IndexOf('{', StringComparison.Ordinal)..]);
        JsonElement data = payload.RootElement;
        Assert.Equal(recipe.Title, data.GetProperty("title").GetString());
        Assert.Equal(recipe.Description, data.GetProperty("description").GetString());
        Assert.Equal(recipe.Advice, data.GetProperty("advice").GetString());
        Assert.Equal(recipe.Servings, data.GetProperty("servings").GetInt32());
        Assert.Equal("Тыква", data.GetProperty("ingredients")[0].GetProperty("productName").GetString());
        Assert.Equal(500, data.GetProperty("ingredients")[0].GetProperty("amount").GetInt32());
        Assert.Equal("Очистить", data.GetProperty("ingredients")[0].GetProperty("comment").GetString());
        Assert.Equal(2, data.GetProperty("steps").GetArrayLength());
        Assert.Equal("Измельчить в пюре.", data.GetProperty("steps")[1].GetProperty("text").GetString());
    }


    private sealed class RecordingHandler : HttpMessageHandler
    {
        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request.Content);
            RequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"created":1,"data":[{"b64_json":"AQID"}]}""", Encoding.UTF8, "application/json")
            };
        }
    }
}
