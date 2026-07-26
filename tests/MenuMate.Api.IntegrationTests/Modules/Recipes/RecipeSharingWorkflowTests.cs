using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MenuMate.Contracts.MenuPlanning;
using MenuMate.Contracts.Recipes;

namespace MenuMate.Api.IntegrationTests;

public sealed class RecipeSharingWorkflowTests : IAsyncLifetime, IDisposable
{
    private readonly MenuMateApiFactory _factory = new();

    public Task InitializeAsync() => _factory.InitializeAsync();

    public Task DisposeAsync() => _factory.DisposeAsync();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task PublicRecipeCanBeSavedAndCopiedButNotEditedByAnotherUser()
    {
        using HttpClient ownerHttpClient = _factory.CreateClient();
        using HttpClient readerHttpClient = _factory.CreateClient();
        var owner = new ApiTestClient(ownerHttpClient);
        var reader = new ApiTestClient(readerHttpClient);
        await owner.RegisterAsync(TestEmail.Create("sharing-owner"));
        await reader.RegisterAsync(TestEmail.Create("sharing-reader"));

        RecipeResponse original = await CreateRecipeAsync(ownerHttpClient, "Shared pasta", "Public");

        RecipeResponse? visible = await readerHttpClient.GetFromJsonAsync<RecipeResponse>(
            $"/api/recipes/{original.Id}");
        Assert.NotNull(visible);
        Assert.False(visible.IsOwnedByCurrentUser);
        Assert.False(visible.IsFavorite);
        Assert.Equal(0, visible.FavoriteCount);

        HttpResponseMessage saveResponse = await readerHttpClient.PostAsync(
            RelativeUri($"/api/recipes/{original.Id}/favorite?revisionId={original.RevisionId}"),
            content: null);
        saveResponse.EnsureSuccessStatusCode();

        RecipeListPageResponse? library = await readerHttpClient.GetFromJsonAsync<RecipeListPageResponse>(
            "/api/recipes?scope=library");
        Assert.NotNull(library);
        RecipeListItemResponse saved = Assert.Single(library.Items);
        Assert.Equal(original.Id, saved.Id);
        Assert.True(saved.IsFavorite);
        Assert.Equal(1, saved.FavoriteCount);
        Assert.Equal(original.RevisionId, saved.RevisionId);
        Assert.Equal(1, library.TotalCount);

        RecipeResponse? savedDetails = await readerHttpClient.GetFromJsonAsync<RecipeResponse>(
            $"/api/recipes/{original.Id}");
        Assert.NotNull(savedDetails);
        Assert.Equal(1, savedDetails.FavoriteCount);

        HttpResponseMessage copyResponse = await readerHttpClient.PostAsJsonAsync(
            $"/api/recipes/{original.Id}/copy",
            new CopyRecipeRequest(original.RevisionId, CreateRequest(original.Title, "Private"), true));
        copyResponse.EnsureSuccessStatusCode();
        RecipeResponse? copy = await copyResponse.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(copy);
        Assert.NotEqual(original.Id, copy.Id);
        Assert.Equal(original.Id, copy.SourceRecipeId);
        Assert.Equal(original.RevisionId, copy.SourceRevisionId);
        Assert.Equal(original.Title, copy.Title);
        Assert.Equal("Private", copy.Visibility);
        Assert.True(copy.IsOwnedByCurrentUser);

        HttpResponseMessage editOriginalResponse = await readerHttpClient.PutAsJsonAsync(
            $"/api/recipes/{original.Id}",
            CreateRequest("Changed by reader", "Public"));
        await ProblemDetailsAssert.HasProblemAsync(
            editOriginalResponse,
            HttpStatusCode.Forbidden,
            "Recipes.AccessDenied");
    }

    [Fact]
    public async Task CatalogShouldFilterByOwnershipAndSortByPopularity()
    {
        using HttpClient ownerHttpClient = _factory.CreateClient();
        using HttpClient readerHttpClient = _factory.CreateClient();
        var owner = new ApiTestClient(ownerHttpClient);
        var reader = new ApiTestClient(readerHttpClient);
        await owner.RegisterAsync(TestEmail.Create("catalog-sort-owner"));
        await reader.RegisterAsync(TestEmail.Create("catalog-sort-reader"));

        RecipeResponse alphabetic = await CreateRecipeAsync(ownerHttpClient, "Alpha recipe", "Public");
        RecipeResponse popular = await CreateRecipeAsync(ownerHttpClient, "Popular recipe", "Public");
        RecipeResponse ownRecipe = await CreateRecipeAsync(readerHttpClient, "Reader recipe", "Public");

        (await ownerHttpClient.PostAsync(RelativeUri($"/api/recipes/{popular.Id}/favorite"), null))
            .EnsureSuccessStatusCode();
        (await readerHttpClient.PostAsync(RelativeUri($"/api/recipes/{popular.Id}/favorite"), null))
            .EnsureSuccessStatusCode();

        RecipeListPageResponse? mine = await readerHttpClient.GetFromJsonAsync<RecipeListPageResponse>(
            "/api/recipes?scope=catalog&ownership=mine");
        RecipeListPageResponse? others = await readerHttpClient.GetFromJsonAsync<RecipeListPageResponse>(
            "/api/recipes?scope=catalog&ownership=others");
        RecipeListPageResponse? favorites = await readerHttpClient.GetFromJsonAsync<RecipeListPageResponse>(
            "/api/recipes?scope=catalog&ownership=others&favoritesOnly=true");
        RecipeListPageResponse? sorted = await readerHttpClient.GetFromJsonAsync<RecipeListPageResponse>(
            "/api/recipes?scope=catalog&sort=popular");

        Assert.NotNull(mine);
        Assert.NotNull(others);
        Assert.NotNull(favorites);
        Assert.NotNull(sorted);
        Assert.Equal(ownRecipe.Id, Assert.Single(mine.Items).Id);
        Assert.Equal(2, others.Items.Count);
        Assert.Contains(others.Items, recipe => recipe.Id == alphabetic.Id);
        Assert.Contains(others.Items, recipe => recipe.Id == popular.Id);
        Assert.Equal(popular.Id, Assert.Single(favorites.Items).Id);
        Assert.Equal(popular.Id, sorted.Items.First().Id);
        Assert.Equal(2, sorted.Items.First().FavoriteCount);
        Assert.Equal(1, mine.TotalCount);
        Assert.Equal(2, others.TotalCount);
        Assert.Equal(1, favorites.TotalCount);
        Assert.Equal(3, sorted.TotalCount);
    }

    [Fact]
    public async Task PrivateRecipeCannotBeReadByAnotherUser()
    {
        using HttpClient ownerHttpClient = _factory.CreateClient();
        using HttpClient readerHttpClient = _factory.CreateClient();
        var owner = new ApiTestClient(ownerHttpClient);
        var reader = new ApiTestClient(readerHttpClient);
        await owner.RegisterAsync(TestEmail.Create("private-owner"));
        await reader.RegisterAsync(TestEmail.Create("private-reader"));

        RecipeResponse recipe = await CreateRecipeAsync(ownerHttpClient, "Private soup", "Private");
        HttpResponseMessage response = await readerHttpClient.GetAsync(
            RelativeUri($"/api/recipes/{recipe.Id}"));

        await ProblemDetailsAssert.HasProblemAsync(response, HttpStatusCode.NotFound, "Recipes.NotFound");
    }

    [Fact]
    public async Task SavedRevisionShouldReportUpdateAndRemainReadableWhenSourceBecomesUnavailable()
    {
        using HttpClient ownerHttpClient = _factory.CreateClient();
        using HttpClient readerHttpClient = _factory.CreateClient();
        var owner = new ApiTestClient(ownerHttpClient);
        var reader = new ApiTestClient(readerHttpClient);
        await owner.RegisterAsync(TestEmail.Create("saved-revision-owner"));
        await reader.RegisterAsync(TestEmail.Create("saved-revision-reader"));

        RecipeResponse original = await CreateRecipeAsync(ownerHttpClient, "Original pasta", "Public");
        (await readerHttpClient.PostAsync(
            RelativeUri($"/api/recipes/{original.Id}/favorite?revisionId={original.RevisionId}"),
            null)).EnsureSuccessStatusCode();

        CreateRecipeRequest updatedRequest = CreateRequest("Updated pasta", "Public");
        (await ownerHttpClient.PutAsJsonAsync($"/api/recipes/{original.Id}", updatedRequest))
            .EnsureSuccessStatusCode();
        RecipeResponse? current = await ownerHttpClient.GetFromJsonAsync<RecipeResponse>(
            $"/api/recipes/{original.Id}");
        Assert.NotNull(current);
        Assert.NotEqual(original.RevisionId, current.RevisionId);

        RecipeResponse? savedRevision = await readerHttpClient.GetFromJsonAsync<RecipeResponse>(
            $"/api/recipes/{original.Id}?revisionId={original.RevisionId}");
        Assert.NotNull(savedRevision);
        Assert.Equal("Original pasta", savedRevision.Title);
        Assert.Equal("UpdateAvailable", savedRevision.RevisionState);
        Assert.Equal(current.RevisionId, savedRevision.CurrentRevisionId);
        Assert.True(savedRevision.IsDisplayedRevisionSaved);

        RecipeListPageResponse? library = await readerHttpClient.GetFromJsonAsync<RecipeListPageResponse>(
            "/api/recipes?scope=library");
        Assert.NotNull(library);
        RecipeListItemResponse savedCard = Assert.Single(library.Items);
        Assert.Equal(original.RevisionId, savedCard.RevisionId);
        Assert.Equal("UpdateAvailable", savedCard.RevisionState);

        RecipeResponse? newRevision = await readerHttpClient.GetFromJsonAsync<RecipeResponse>(
            $"/api/recipes/{original.Id}?revisionId={current.RevisionId}");
        Assert.NotNull(newRevision);
        Assert.Equal("Current", newRevision.RevisionState);
        Assert.True(newRevision.IsFavorite);
        Assert.False(newRevision.IsDisplayedRevisionSaved);

        (await readerHttpClient.PostAsync(
            RelativeUri($"/api/recipes/{original.Id}/favorite?revisionId={current.RevisionId}"),
            null)).EnsureSuccessStatusCode();
        RecipeResponse? oldAfterUpdate = await readerHttpClient.GetFromJsonAsync<RecipeResponse>(
            $"/api/recipes/{original.Id}?revisionId={original.RevisionId}");
        Assert.NotNull(oldAfterUpdate);
        Assert.Equal("Historical", oldAfterUpdate.RevisionState);
        Assert.False(oldAfterUpdate.IsDisplayedRevisionSaved);

        CreateRecipeRequest privateRequest = updatedRequest with { Visibility = "Private" };
        (await ownerHttpClient.PutAsJsonAsync($"/api/recipes/{original.Id}", privateRequest))
            .EnsureSuccessStatusCode();
        RecipeResponse? ownerAfterVisibilityChange = await ownerHttpClient.GetFromJsonAsync<RecipeResponse>(
            $"/api/recipes/{original.Id}");
        Assert.NotNull(ownerAfterVisibilityChange);
        Assert.Equal(current.RevisionId, ownerAfterVisibilityChange.RevisionId);

        RecipeResponse? unavailable = await readerHttpClient.GetFromJsonAsync<RecipeResponse>(
            $"/api/recipes/{original.Id}?revisionId={current.RevisionId}");
        Assert.NotNull(unavailable);
        Assert.Equal("SourceUnavailable", unavailable.RevisionState);
        Assert.Null(unavailable.CurrentRevisionId);
        Assert.Empty(unavailable.Images);

        HttpResponseMessage copyResponse = await readerHttpClient.PostAsJsonAsync(
            $"/api/recipes/{original.Id}/copy",
            new CopyRecipeRequest(
                current.RevisionId,
                CreateRequest("Reader copy", "Private"),
                CopySourceCover: true));
        copyResponse.EnsureSuccessStatusCode();
        RecipeResponse? copy = await copyResponse.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(copy);
        Assert.Equal(current.RevisionId, copy.SourceRevisionId);
        Assert.Equal("Reader copy", copy.Title);

        (await readerHttpClient.DeleteAsync(RelativeUri($"/api/recipes/{original.Id}/favorite")))
            .EnsureSuccessStatusCode();
        HttpResponseMessage removedSnapshot = await readerHttpClient.GetAsync(
            RelativeUri($"/api/recipes/{original.Id}?revisionId={current.RevisionId}"));
        await ProblemDetailsAssert.HasProblemAsync(
            removedSnapshot,
            HttpStatusCode.NotFound,
            "Recipes.NotFound");
    }

    [Fact]
    public async Task MenuRevisionShouldRemainOpenAndEditableAfterOwnerDeletesRecipe()
    {
        using HttpClient ownerHttpClient = _factory.CreateClient();
        using HttpClient readerHttpClient = _factory.CreateClient();
        var owner = new ApiTestClient(ownerHttpClient);
        var reader = new ApiTestClient(readerHttpClient);
        await owner.RegisterAsync(TestEmail.Create("deleted-menu-owner"));
        await reader.RegisterAsync(TestEmail.Create("deleted-menu-reader"));

        RecipeResponse recipe = await CreateRecipeAsync(ownerHttpClient, "Canonical title", "Public");
        Guid dinnerSlotId = await GetDinnerSlotIdAsync(readerHttpClient);
        HttpResponseMessage addResponse = await AddRecipeToMenuAsync(
            readerHttpClient,
            dinnerSlotId,
            recipe.Id,
            recipe.RevisionId);
        addResponse.EnsureSuccessStatusCode();
        MenuCalendarItemResponse? menuItem = await addResponse.Content.ReadFromJsonAsync<MenuCalendarItemResponse>();
        Assert.NotNull(menuItem);
        Assert.Equal("Canonical title", menuItem.RecipeTitle);

        (await ownerHttpClient.DeleteAsync(RelativeUri($"/api/recipes/{recipe.Id}")))
            .EnsureSuccessStatusCode();

        RecipeResponse? snapshot = await readerHttpClient.GetFromJsonAsync<RecipeResponse>(
            $"/api/recipes/{recipe.Id}?revisionId={recipe.RevisionId}");
        Assert.NotNull(snapshot);
        Assert.Equal("SourceUnavailable", snapshot.RevisionState);

        HttpResponseMessage updateMenuResponse = await readerHttpClient.PutAsJsonAsync(
            $"/api/menu-calendar/items/{menuItem.Id}",
            new UpdateMenuCalendarItemRequest(
                new DateOnly(2026, 6, 2),
                dinnerSlotId,
                null,
                3,
                "После удаления"));
        updateMenuResponse.EnsureSuccessStatusCode();
        MenuCalendarItemResponse? updatedMenuItem =
            await updateMenuResponse.Content.ReadFromJsonAsync<MenuCalendarItemResponse>();
        Assert.NotNull(updatedMenuItem);
        Assert.Equal(recipe.RevisionId, updatedMenuItem.RecipeRevisionId);
        Assert.Equal("Canonical title", updatedMenuItem.RecipeTitle);
        Assert.Equal(3, updatedMenuItem.Servings);

        HttpResponseMessage copyResponse = await readerHttpClient.PostAsJsonAsync(
            $"/api/recipes/{recipe.Id}/copy",
            new CopyRecipeRequest(
                recipe.RevisionId,
                CreateRequest("Menu copy", "Private"),
                CopySourceCover: true));
        copyResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task CopyShouldPhysicallyCopyAccessibleSourceCover()
    {
        using HttpClient ownerHttpClient = _factory.CreateClient();
        using HttpClient readerHttpClient = _factory.CreateClient();
        var owner = new ApiTestClient(ownerHttpClient);
        var reader = new ApiTestClient(readerHttpClient);
        await owner.RegisterAsync(TestEmail.Create("cover-copy-owner"));
        await reader.RegisterAsync(TestEmail.Create("cover-copy-reader"));

        RecipeResponse source = await CreateRecipeAsync(ownerHttpClient, "Recipe with cover", "Public");
        byte[] sourceBytes = [0xff, 0xd8, 0xff, 0xd9];
        using var multipart = new MultipartFormDataContent();
        using var imageContent = new ByteArrayContent(sourceBytes);
        using var scopeContent = new StringContent("Cover");
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        multipart.Add(imageContent, "file", "cover.jpg");
        multipart.Add(scopeContent, "scope");
        HttpResponseMessage uploadResponse = await ownerHttpClient.PostAsync(
            RelativeUri($"/api/recipes/{source.Id}/images"),
            multipart);
        uploadResponse.EnsureSuccessStatusCode();

        HttpResponseMessage copyResponse = await readerHttpClient.PostAsJsonAsync(
            $"/api/recipes/{source.Id}/copy",
            new CopyRecipeRequest(
                source.RevisionId,
                CreateRequest("Copied with cover", "Private"),
                CopySourceCover: true));
        copyResponse.EnsureSuccessStatusCode();
        RecipeResponse? createdCopy = await copyResponse.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(createdCopy);

        RecipeResponse? sourceWithCover = await ownerHttpClient.GetFromJsonAsync<RecipeResponse>(
            $"/api/recipes/{source.Id}");
        RecipeResponse? copyWithCover = await readerHttpClient.GetFromJsonAsync<RecipeResponse>(
            $"/api/recipes/{createdCopy.Id}");
        Assert.NotNull(sourceWithCover);
        Assert.NotNull(copyWithCover);
        RecipeImageResponse sourceCover = Assert.Single(sourceWithCover.Images);
        RecipeImageResponse copiedCover = Assert.Single(copyWithCover.Images);
        Assert.NotEqual(sourceCover.ObjectKey, copiedCover.ObjectKey);
        Assert.Equal(
            _factory.ObjectStorage.GetBytes(sourceCover.BucketName, sourceCover.ObjectKey),
            _factory.ObjectStorage.GetBytes(copiedCover.BucketName, copiedCover.ObjectKey));
    }

    [Fact]
    public async Task MenuRejectsPrivateForeignRecipeAndMismatchedRevision()
    {
        using HttpClient ownerHttpClient = _factory.CreateClient();
        using HttpClient readerHttpClient = _factory.CreateClient();
        var owner = new ApiTestClient(ownerHttpClient);
        var reader = new ApiTestClient(readerHttpClient);
        await owner.RegisterAsync(TestEmail.Create("menu-recipe-owner"));
        await reader.RegisterAsync(TestEmail.Create("menu-recipe-reader"));

        RecipeResponse first = await CreateRecipeAsync(ownerHttpClient, "First private recipe", "Private");
        RecipeResponse second = await CreateRecipeAsync(ownerHttpClient, "Second private recipe", "Private");
        Guid readerDinnerSlotId = await GetDinnerSlotIdAsync(readerHttpClient);
        Guid ownerDinnerSlotId = await GetDinnerSlotIdAsync(ownerHttpClient);

        HttpResponseMessage privateRecipeResponse = await AddRecipeToMenuAsync(
            readerHttpClient,
            readerDinnerSlotId,
            first.Id,
            first.RevisionId);
        await ProblemDetailsAssert.HasProblemAsync(
            privateRecipeResponse,
            HttpStatusCode.Forbidden,
            "MenuPlanning.AccessDenied");

        HttpResponseMessage mismatchedRevisionResponse = await AddRecipeToMenuAsync(
            ownerHttpClient,
            ownerDinnerSlotId,
            first.Id,
            second.RevisionId);
        await ProblemDetailsAssert.HasProblemAsync(
            mismatchedRevisionResponse,
            HttpStatusCode.Forbidden,
            "MenuPlanning.AccessDenied");
    }

    [Fact]
    public async Task CatalogAllowsSameProductNameInDifferentCategories()
    {
        using HttpClient httpClient = _factory.CreateClient();
        var user = new ApiTestClient(httpClient);
        await user.RegisterAsync(TestEmail.Create("product-category-variants"));

        RecipeResponse first = await CreateRecipeAsync(
            httpClient,
            CreateRequestWithIngredient("Chicken as other", "Chicken", "Other"));
        RecipeResponse second = await CreateRecipeAsync(
            httpClient,
            CreateRequestWithIngredient("Chicken as meat", "Chicken", "MeatAndPoultry"));

        IngredientResponse firstIngredient = Assert.Single(first.Ingredients);
        IngredientResponse secondIngredient = Assert.Single(second.Ingredients);
        Assert.NotEqual(firstIngredient.IngredientId, secondIngredient.IngredientId);
        Assert.Equal("Other", firstIngredient.Category);
        Assert.Equal("MeatAndPoultry", secondIngredient.Category);
    }

    private static async Task<RecipeResponse> CreateRecipeAsync(
        HttpClient client,
        string title,
        string visibility)
    {
        return await CreateRecipeAsync(client, CreateRequest(title, visibility));
    }

    private static async Task<RecipeResponse> CreateRecipeAsync(
        HttpClient client,
        CreateRecipeRequest request)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/recipes/", request);
        response.EnsureSuccessStatusCode();

        RecipeResponse? recipe = await response.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(recipe);
        return recipe;
    }

    private static CreateRecipeRequest CreateRequest(string title, string visibility) =>
        new(
            title,
            "Recipe sharing test",
            2,
            "MainCourse",
            visibility,
            30,
            15,
            null,
            [
                new RecipeIngredientRequest(
                    null,
                    "Pasta",
                    200m,
                    "Gram",
                    "GrainsAndPasta",
                    null,
                    false)
            ],
            [new PreparationStepRequest("Cook")],
            []);

    private static CreateRecipeRequest CreateRequestWithIngredient(
        string title,
        string productName,
        string category) =>
        new(
            title,
            "Product category variant test",
            2,
            "MainCourse",
            "Private",
            30,
            15,
            null,
            [
                new RecipeIngredientRequest(
                    null,
                    productName,
                    200m,
                    "Gram",
                    category,
                    null,
                    false)
            ],
            [new PreparationStepRequest("Cook")],
            []);

    private static async Task<Guid> GetDinnerSlotIdAsync(HttpClient client)
    {
        MenuCalendarResponse? calendar = await client.GetFromJsonAsync<MenuCalendarResponse>(
            "/api/menu-calendar?startDate=2026-06-01&endDate=2026-06-07");
        Assert.NotNull(calendar);
        return Assert.Single(calendar.MealSlots, slot => slot.Name == "Ужин").Id;
    }

    private static Task<HttpResponseMessage> AddRecipeToMenuAsync(
        HttpClient client,
        Guid mealSlotId,
        Guid recipeId,
        Guid recipeRevisionId) =>
        client.PostAsJsonAsync(
            "/api/menu-calendar/items",
            new CreateMenuCalendarItemRequest(
                new DateOnly(2026, 6, 1),
                mealSlotId,
                recipeId,
                recipeRevisionId,
                null,
                2,
                null));

    private static Uri RelativeUri(string path) => new(path, UriKind.Relative);
}
