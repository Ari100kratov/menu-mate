using MenuMate.Common.Application;
using MenuMate.Contracts.Recipes;

namespace MenuMate.Modules.Recipes.Application.UploadRecipeImage;

/// <summary>
/// Загружает изображение рецепта и необязательную атрибуцию внешнего источника.
/// </summary>
/// <param name="RecipeId">Идентификатор рецепта.</param>
/// <param name="Content">Содержимое изображения.</param>
/// <param name="FileName">Исходное имя файла.</param>
/// <param name="ContentType">MIME-тип изображения.</param>
/// <param name="ContentLength">Размер изображения в байтах.</param>
/// <param name="Scope">Область привязки: Cover или Step.</param>
/// <param name="StepNumber">Номер шага приготовления.</param>
/// <param name="AltText">Альтернативный текст.</param>
/// <param name="SourceUrl">Страница исходного изображения.</param>
/// <param name="AuthorName">Имя автора исходного изображения.</param>
/// <param name="LicenseName">Название лицензии исходного изображения.</param>
/// <param name="LicenseUrl">Ссылка на текст лицензии.</param>
public sealed record UploadRecipeImageCommand(
    Guid RecipeId,
    Stream Content,
    string FileName,
    string ContentType,
    long ContentLength,
    string? Scope,
    int? StepNumber,
    string? AltText,
    Uri? SourceUrl = null,
    string? AuthorName = null,
    string? LicenseName = null,
    Uri? LicenseUrl = null) : ICommand<RecipeImageResponse>;
