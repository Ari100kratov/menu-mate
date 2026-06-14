using MenuMate.Modules.RecipeImports.Domain.Enums;
using MenuMate.Modules.RecipeImports.Domain.Errors;
using MenuMate.Modules.RecipeImports.Domain.Models;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.RecipeImports.Domain.UnitTests.Models;

public sealed class RecipeImportDraftTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 6, 13, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateShouldInitializeReadyDraftAndNormalizeTitle()
    {
        RecipeImportDraft draft = CreateDraft("  Паста  ");

        Assert.Equal(RecipeImportDraftStatus.Ready, draft.Status);
        Assert.Equal("Паста", draft.Title);
        Assert.Null(draft.CreatedRecipeId);
        Assert.Equal(FixedNow, draft.CreatedAt);
        Assert.Equal(FixedNow, draft.UpdatedAt);
    }

    [Fact]
    public void UpdateShouldReplaceEditableSnapshot()
    {
        RecipeImportDraft draft = CreateDraft("Паста");
        DateTimeOffset updatedAt = FixedNow.AddMinutes(1);

        Result result = draft.Update("  Новый заголовок  ", """{"title":"updated"}""", updatedAt);

        Assert.True(result.IsSuccess);
        Assert.Equal("Новый заголовок", draft.Title);
        Assert.Equal("""{"title":"updated"}""", draft.RecipeJson);
        Assert.Equal(updatedAt, draft.UpdatedAt);
    }

    [Fact]
    public void ConfirmShouldMakeFurtherUpdatesInvalid()
    {
        RecipeImportDraft draft = CreateDraft("Паста");
        var createdRecipeId = RecipeId.Create();

        draft.Confirm(createdRecipeId, FixedNow.AddMinutes(1));
        Result result = draft.Update("Изменение", "{}", FixedNow.AddMinutes(2));

        Assert.Equal(RecipeImportDraftStatus.Confirmed, draft.Status);
        Assert.Equal(createdRecipeId, draft.CreatedRecipeId);
        Assert.True(result.IsFailure);
        Assert.Equal(RecipeImportDraftErrors.AlreadyConfirmed, result.Error);
    }

    [Fact]
    public void CreateShouldExposeAllSourceImagesInUploadOrder()
    {
        var additionalImage = new RecipeImportSourceImage(
            "imports",
            "users/source-2.png",
            "image/png",
            200,
            "source-2.png");

        RecipeImportDraft draft = CreateDraft("Паста", [additionalImage]);

        Assert.Collection(
            draft.SourceImages,
            image => Assert.Equal("users/source.png", image.ObjectKey),
            image => Assert.Equal(additionalImage, image));
    }

    private static RecipeImportDraft CreateDraft(
        string title,
        IReadOnlyCollection<RecipeImportSourceImage>? additionalSourceImages = null) =>
        RecipeImportDraft.Create(
            ImportDraftId.Create(),
            UserId.Create(),
            RecipeId.Create(),
            title,
            "{}",
            "{}",
            "imports",
            "users/source.png",
            "image/png",
            100,
            "source.png",
            additionalSourceImages ?? [],
            FixedNow);
}
