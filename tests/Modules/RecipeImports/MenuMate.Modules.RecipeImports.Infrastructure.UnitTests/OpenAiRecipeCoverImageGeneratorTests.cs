using System.ClientModel;
using System.ClientModel.Primitives;
using System.Net;
using System.Text;
using System.Text.Json;
using MenuMate.Contracts.Recipes;
using MenuMate.Modules.RecipeImports.Application.Generation;
using MenuMate.Modules.RecipeImports.Infrastructure.OpenAI;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Images;

namespace MenuMate.Modules.RecipeImports.Infrastructure.UnitTests;

public sealed class OpenAiRecipeCoverImageGeneratorTests
{
    [Theory]
    [InlineData(832, "medium")]
    [InlineData(1024, "high")]
    public async Task GenerateAsyncSendsOnlyVisualRecipeDataAndConfiguredOptionsReturnsJpeg(int size, string quality)
    {
        CreateRecipeRequest recipe = new(
            "Тыквенный суп", "Густой крем-суп", 2, "Soup", "Private", 40, 15, null,
            [new RecipeIngredientRequest(null, "Тыква", 500, "Gram", "Vegetables", "Очистить", false)],
            [new PreparationStepRequest("Запечь тыкву."), new PreparationStepRequest("Измельчить в пюре.")],
            ["осенний"], "Подавать горячим.");
        using RecordingHandler handler = new();
        using HttpClient httpClient = new(handler);
        OpenAiRecipeCoverImageGeneratorOptions options = new() { ApiKey = "test-key", ImageSize = size, Quality = quality };
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
        Assert.Equal(quality, root.GetProperty("quality").GetString());
        Assert.Equal($"{size}x{size}", root.GetProperty("size").GetString());
        Assert.Equal("jpeg", root.GetProperty("output_format").GetString());
        Assert.Equal(90, root.GetProperty("output_compression").GetInt32());
        string prompt = Assert.IsType<string>(root.GetProperty("prompt").GetString());
        using var payload = JsonDocument.Parse(prompt[prompt.IndexOf('{', StringComparison.Ordinal)..]);
        JsonElement data = payload.RootElement;
        Assert.Equal(recipe.Title, data.GetProperty("title").GetString());
        Assert.Equal(recipe.Description, data.GetProperty("description").GetString());
        Assert.Equal(
            ["title", "description", "ingredients", "steps", "tags"],
            data.EnumerateObject().Select(property => property.Name));
        Assert.Equal(
            ["productName", "amount", "unit", "comment", "isOptional"],
            data.GetProperty("ingredients")[0].EnumerateObject().Select(property => property.Name));
        Assert.Equal("Тыква", data.GetProperty("ingredients")[0].GetProperty("productName").GetString());
        Assert.Equal(500, data.GetProperty("ingredients")[0].GetProperty("amount").GetInt32());
        Assert.Equal("Очистить", data.GetProperty("ingredients")[0].GetProperty("comment").GetString());
        Assert.Equal(2, data.GetProperty("steps").GetArrayLength());
        Assert.Equal("Измельчить в пюре.", data.GetProperty("steps")[1].GetProperty("text").GetString());
        Assert.Equal("осенний", data.GetProperty("tags")[0].GetString());
    }


    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GenerateAsyncDistinguishesDeadlineFromCallerCancellation(bool cancelFromCaller)
    {
        using RecordingHandler handler = new(waitForCancellation: true);
        using HttpClient httpClient = new(handler);
        OpenAiRecipeCoverImageGeneratorOptions options = new()
        {
            ApiKey = "test-key",
            RequestTimeout = cancelFromCaller ? TimeSpan.FromSeconds(10) : TimeSpan.FromMilliseconds(50)
        };
        ImageClient client = new(options.Model, new ApiKeyCredential(options.ApiKey),
            new OpenAIClientOptions
            {
                Transport = new HttpClientPipelineTransport(httpClient),
                RetryPolicy = new ClientRetryPolicy(0)
            });
        OpenAiRecipeCoverImageGenerator generator = new(client, options,
            NullLogger<OpenAiRecipeCoverImageGenerator>.Instance);
        CreateRecipeRequest recipe = new("Суп", null, 2, "Soup", "Private", null, null, null, [], [], []);
        using CancellationTokenSource cancellation = new();
        if (cancelFromCaller)
        {
            cancellation.CancelAfter(TimeSpan.FromMilliseconds(50));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => generator.GenerateAsync(recipe, cancellation.Token));
        }
        else
        {
            RecipeCoverImageGenerationException exception = await Assert.ThrowsAsync<RecipeCoverImageGenerationException>(
                () => generator.GenerateAsync(recipe, cancellation.Token));
            Assert.Contains("слишком много времени", exception.Message, StringComparison.Ordinal);
            Assert.False(cancellation.IsCancellationRequested);
        }
    }

    [Theory]
    [InlineData(null, null, 832, "medium")]
    [InlineData("1024", "high", 1024, "high")]
    public void CreateImageOptionsReadsConfiguration(string? size, string? quality, int expectedSize, string expectedQuality)
    {
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["OpenAI:ImageSize"] = size, ["OpenAI:ImageQuality"] = quality }).Build();
        OpenAiRecipeCoverImageGeneratorOptions options = RecipeImportsInfrastructureDependencyInjection.CreateImageOptions(configuration);
        Assert.Equal(expectedSize, options.ImageSize);
        Assert.Equal(expectedQuality, options.Quality);
    }

    [Theory]
    [InlineData("512", "medium")]
    [InlineData("833", "medium")]
    [InlineData("4096", "medium")]
    [InlineData("invalid", "medium")]
    [InlineData("832", "invalid")]
    public void CreateImageOptionsRejectsInvalidConfiguration(string size, string quality)
    {
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["OpenAI:ImageSize"] = size, ["OpenAI:ImageQuality"] = quality }).Build();
        Assert.Throws<InvalidOperationException>(
            () => RecipeImportsInfrastructureDependencyInjection.CreateImageOptions(configuration));
    }

    private sealed class RecordingHandler(bool waitForCancellation = false) : HttpMessageHandler
    {
        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (waitForCancellation)
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }

            ArgumentNullException.ThrowIfNull(request.Content);
            RequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"created":1,"data":[{"b64_json":"AQID"}]}""", Encoding.UTF8, "application/json")
            };
        }
    }
}
