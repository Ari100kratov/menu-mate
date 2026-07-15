using MenuMate.Common.Application;

namespace MenuMate.Modules.RecipeImports.Application.GetRecipeImportSourceImage;

/// <summary>Запрос содержимого исходного изображения черновика импорта.</summary>
public sealed record GetRecipeImportSourceImageQuery(Guid DraftId, int SourceImageIndex)
    : IQuery<RecipeImportSourceImageContent>;

/// <summary>Поток исходного изображения и его метаданные.</summary>
public sealed record RecipeImportSourceImageContent(
    Stream Content,
    string ContentType,
    string FileName);
