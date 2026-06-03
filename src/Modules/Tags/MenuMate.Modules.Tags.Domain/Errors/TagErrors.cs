using MenuMate.SharedKernel;

namespace MenuMate.Modules.Tags.Domain.Errors;

/// <summary>
/// Ошибки домена тегов.
/// </summary>
public static class TagErrors
{
    /// <summary>
    /// Название тега пустое.
    /// </summary>
    public static readonly AppError EmptyName = AppError.Validation(
        "Tags.EmptyName",
        "Название тега не может быть пустым.");

    /// <summary>
    /// Название тега слишком длинное.
    /// </summary>
    public static readonly AppError NameTooLong = AppError.Validation(
        "Tags.NameTooLong",
        "Название тега не должно быть длиннее 64 символов.");
}


