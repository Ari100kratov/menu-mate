using MenuMate.Common.Application;
using MenuMate.Modules.RecipeImports.Application.Generation;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.RecipeImports.Application.GenerateRecipeCoverImage;

internal sealed class GenerateRecipeCoverImageCommandHandler(
    IRecipeCoverImageGenerator generator)
    : ICommandHandler<GenerateRecipeCoverImageCommand, GeneratedRecipeCoverImage>
{
    public async Task<Result<GeneratedRecipeCoverImage>> Handle(
        GenerateRecipeCoverImageCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            GeneratedRecipeCoverImage image = await generator.GenerateAsync(
                command.Recipe,
                cancellationToken);
            return Result.Success(image);
        }
        catch (RecipeCoverImageGenerationException exception)
        {
            return Result.Failure<GeneratedRecipeCoverImage>(
                ImportApplicationErrors.CoverGenerationFailed(exception.Message));
        }
    }
}
