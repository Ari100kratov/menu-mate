using MenuMate.Modules.Recipes.Domain.Errors;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Domain.Models;

/// <summary>
/// Шаг приготовления рецепта.
/// </summary>
public sealed record PreparationStep
{
    private PreparationStep(int number, string text)
    {
        Number = number;
        Text = text;
    }

    /// <summary>
    /// Порядковый номер шага.
    /// </summary>
    public int Number { get; }

    /// <summary>
    /// Текст шага.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Создает шаг приготовления.
    /// </summary>
    public static Result<PreparationStep> Create(int number, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result.Failure<PreparationStep>(RecipeErrors.EmptyStepText);
        }

        return new PreparationStep(number, text.Trim());
    }
}

