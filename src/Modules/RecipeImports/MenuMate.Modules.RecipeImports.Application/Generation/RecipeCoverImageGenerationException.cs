namespace MenuMate.Modules.RecipeImports.Application.Generation;

/// <summary>Ошибка генерации обложки рецепта внешним сервисом.</summary>
public sealed class RecipeCoverImageGenerationException : Exception
{
    /// <summary>Создаёт ошибку генерации.</summary>
    public RecipeCoverImageGenerationException()
    {
    }

    /// <summary>Создаёт ошибку генерации.</summary>
    public RecipeCoverImageGenerationException(string message)
        : base(message)
    {
    }

    /// <summary>Создаёт ошибку генерации с исходным исключением.</summary>
    public RecipeCoverImageGenerationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
