namespace MenuMate.Modules.RecipeImports.Application.Extraction;

/// <summary>
/// Ошибка внешнего сервиса распознавания изображения рецепта.
/// </summary>
public sealed class RecipeImageExtractionException : Exception
{
    /// <summary>Создает ошибку распознавания.</summary>
    public RecipeImageExtractionException()
    {
    }

    /// <summary>Создает ошибку распознавания.</summary>
    public RecipeImageExtractionException(string message)
        : base(message)
    {
    }

    /// <summary>Создает ошибку распознавания с исходным исключением.</summary>
    public RecipeImageExtractionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
