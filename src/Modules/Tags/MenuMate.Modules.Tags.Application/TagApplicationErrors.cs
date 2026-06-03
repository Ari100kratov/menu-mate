using MenuMate.SharedKernel;

namespace MenuMate.Modules.Tags.Application;

internal static class TagApplicationErrors
{
    public static readonly AppError DuplicateName = AppError.Conflict(
        "Tags.DuplicateName",
        "Tag with the same name already exists.");

    public static readonly AppError InvalidKind = AppError.Validation(
        "Tags.InvalidKind",
        "Tag kind is invalid.");

    public static AppError NotFound(Guid tagId) => AppError.NotFound(
        "Tags.NotFound",
        $"Tag with id '{tagId}' was not found.");
}
