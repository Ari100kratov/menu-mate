using MenuMate.Contracts.Recipes;
using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.Modules.Recipes.Domain.Models;
using MenuMate.Modules.Recipes.Domain.ValueObjects;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Application;

internal static class RecipeRequestMapper
{
    public static Result<RecipeDraft> Map(CreateRecipeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return Map(
            request.Title,
            request.Description,
            request.Servings,
            request.Category,
            request.Visibility,
            request.TotalTimeMinutes,
            request.ActiveTimeMinutes,
            request.SourceUrl,
            request.Ingredients,
            request.Steps,
            request.Tags);
    }

    public static Result<RecipeDraft> Map(UpdateRecipeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return Map(
            request.Title,
            request.Description,
            request.Servings,
            request.Category,
            request.Visibility,
            request.TotalTimeMinutes,
            request.ActiveTimeMinutes,
            request.SourceUrl,
            request.Ingredients,
            request.Steps,
            request.Tags);
    }

    private static Result<RecipeDraft> Map(
        string titleValue,
        string? description,
        int servingsValue,
        string categoryValue,
        string visibilityValue,
        int? totalTimeMinutes,
        int? activeTimeMinutes,
        Uri? sourceUrl,
        IReadOnlyCollection<RecipeIngredientRequest> ingredients,
        IReadOnlyCollection<PreparationStepRequest> steps,
        IReadOnlyCollection<string> tags)
    {
        Result<RecipeTitle> title = RecipeTitle.Create(titleValue);
        if (title.IsFailure)
        {
            return Result.Failure<RecipeDraft>(title.Error);
        }

        Result<Servings> servings = Servings.Create(servingsValue);
        if (servings.IsFailure)
        {
            return Result.Failure<RecipeDraft>(servings.Error);
        }

        Result<RecipeCategory> category = ParseRecipeCategory(categoryValue);
        if (category.IsFailure)
        {
            return Result.Failure<RecipeDraft>(category.Error);
        }

        Result<RecipeVisibility> visibility = ParseRecipeVisibility(visibilityValue);
        if (visibility.IsFailure)
        {
            return Result.Failure<RecipeDraft>(visibility.Error);
        }

        Result timing = ValidateTiming(totalTimeMinutes, activeTimeMinutes);
        if (timing.IsFailure)
        {
            return Result.Failure<RecipeDraft>(timing.Error);
        }

        Result<Uri?> parsedSourceUrl = ValidateSourceUrl(sourceUrl);
        if (parsedSourceUrl.IsFailure)
        {
            return Result.Failure<RecipeDraft>(parsedSourceUrl.Error);
        }

        Result<IReadOnlyCollection<RecipeIngredient>> mappedIngredients = MapIngredients(ingredients);
        if (mappedIngredients.IsFailure)
        {
            return Result.Failure<RecipeDraft>(mappedIngredients.Error);
        }

        Result<IReadOnlyCollection<PreparationStep>> mappedSteps = MapSteps(steps);
        if (mappedSteps.IsFailure)
        {
            return Result.Failure<RecipeDraft>(mappedSteps.Error);
        }

        Result<IReadOnlyCollection<string>> mappedTags = MapTags(tags);
        if (mappedTags.IsFailure)
        {
            return Result.Failure<RecipeDraft>(mappedTags.Error);
        }

        return new RecipeDraft(
            title.Value,
            description,
            servings.Value,
            category.Value,
            visibility.Value,
            totalTimeMinutes,
            activeTimeMinutes,
            parsedSourceUrl.Value,
            mappedIngredients.Value,
            mappedSteps.Value,
            mappedTags.Value);
    }

    private static Result<Uri?> ValidateSourceUrl(Uri? sourceUrl)
    {
        if (sourceUrl is null)
        {
            return Result.Success<Uri?>(null);
        }

        return sourceUrl.IsAbsoluteUri
            ? Result.Success<Uri?>(sourceUrl)
            : Result.Failure<Uri?>(AppError.Validation("Recipes.InvalidSourceUrl", "Источник рецепта должен быть абсолютным URL."));
    }

    private static Result<IReadOnlyCollection<RecipeIngredient>> MapIngredients(
        IReadOnlyCollection<RecipeIngredientRequest> ingredients)
    {
        var result = new List<RecipeIngredient>(ingredients.Count);

        foreach (RecipeIngredientRequest ingredient in ingredients)
        {
            Result<IngredientName> name = IngredientName.Create(ingredient.ProductName);
            if (name.IsFailure)
            {
                return Result.Failure<IReadOnlyCollection<RecipeIngredient>>(name.Error);
            }

            Result<IngredientQuantity> quantity = MapQuantity(ingredient);
            if (quantity.IsFailure)
            {
                return Result.Failure<IReadOnlyCollection<RecipeIngredient>>(quantity.Error);
            }

            Result<ProductCategory> category = ParseProductCategory(ingredient.Category);
            if (category.IsFailure)
            {
                return Result.Failure<IReadOnlyCollection<RecipeIngredient>>(category.Error);
            }

            result.Add(new RecipeIngredient(
                ingredient.IngredientId,
                name.Value,
                quantity.Value,
                category.Value,
                ingredient.Comment,
                ingredient.IsOptional));
        }

        return result;
    }

    private static Result<IngredientQuantity> MapQuantity(RecipeIngredientRequest ingredient)
    {
        Result<MeasurementUnit> unit = ParseMeasurementUnit(ingredient.Unit);
        if (unit.IsFailure)
        {
            return Result.Failure<IngredientQuantity>(unit.Error);
        }

        if (unit.Value == MeasurementUnit.ToTaste)
        {
            return IngredientQuantity.ToTaste();
        }

        if (ingredient.Amount is null)
        {
            return Result.Failure<IngredientQuantity>(AppError.Validation(
                "Recipes.AmountRequired",
                "Укажите количество ингредиента или выберите «по вкусу»."));
        }

        return IngredientQuantity.Measured(ingredient.Amount.Value, unit.Value);
    }

    private static Result<IReadOnlyCollection<PreparationStep>> MapSteps(
        IReadOnlyCollection<PreparationStepRequest> steps)
    {
        var result = new List<PreparationStep>(steps.Count);

        for (int index = 0; index < steps.Count; index++)
        {
            Result<PreparationStep> step = PreparationStep.Create(index + 1, steps.ElementAt(index).Text);
            if (step.IsFailure)
            {
                return Result.Failure<IReadOnlyCollection<PreparationStep>>(step.Error);
            }

            result.Add(step.Value);
        }

        return result;
    }

    private static Result<IReadOnlyCollection<string>> MapTags(IReadOnlyCollection<string> tags)
    {
        var result = new List<string>(tags.Count);
        HashSet<string> normalizedValues = [];

        foreach (string tagValue in tags)
        {
            if (string.IsNullOrWhiteSpace(tagValue))
            {
                return Result.Failure<IReadOnlyCollection<string>>(AppError.Validation(
                    "Recipes.EmptyTag",
                    "Тег не может быть пустым."));
            }

            string normalizedValue = TextNormalizer.NormalizeSearchText(tagValue);
            if (normalizedValue.Length > 64)
            {
                return Result.Failure<IReadOnlyCollection<string>>(AppError.Validation(
                    "Recipes.TagTooLong",
                    "Название тега не может быть длиннее 64 символов."));
            }

            if (normalizedValues.Add(normalizedValue))
            {
                result.Add(tagValue.Trim());
            }
        }

        return result;
    }

    private static Result<ProductCategory> ParseProductCategory(string value) =>
        Enum.TryParse(value, ignoreCase: true, out ProductCategory category)
            ? category
            : Result.Failure<ProductCategory>(AppError.Validation(
                "Recipes.InvalidProductCategory",
                "Категория продукта указана в неизвестном формате."));

    private static Result<RecipeCategory> ParseRecipeCategory(string value) =>
        Enum.TryParse(value, ignoreCase: true, out RecipeCategory category)
            ? category
            : Result.Failure<RecipeCategory>(AppError.Validation(
                "Recipes.InvalidRecipeCategory",
                "Категория рецепта указана в неизвестном формате."));

    private static Result<RecipeVisibility> ParseRecipeVisibility(string value) =>
        Enum.TryParse(value, ignoreCase: true, out RecipeVisibility visibility)
            ? visibility
            : Result.Failure<RecipeVisibility>(AppError.Validation(
                "Recipes.InvalidVisibility",
                "Recipe visibility must be Private or Public."));

    private static Result ValidateTiming(int? totalTimeMinutes, int? activeTimeMinutes)
    {
        if (totalTimeMinutes is <= 0 or > 10080 || activeTimeMinutes is <= 0 or > 10080)
        {
            return Result.Failure(AppError.Validation(
                "Recipes.InvalidCookingTime",
                "Время приготовления должно быть от 1 минуты до 7 дней."));
        }

        if (totalTimeMinutes is not null &&
            activeTimeMinutes is not null &&
            activeTimeMinutes > totalTimeMinutes)
        {
            return Result.Failure(AppError.Validation(
                "Recipes.ActiveTimeExceedsTotalTime",
                "Активное время не может быть больше общего времени приготовления."));
        }

        return Result.Success();
    }

    private static Result<MeasurementUnit> ParseMeasurementUnit(string value) =>
        NormalizeUnit(value) is { } normalizedUnit
            ? normalizedUnit
            : Result.Failure<MeasurementUnit>(AppError.Validation(
                "Recipes.InvalidMeasurementUnit",
                "Единица измерения указана в неизвестном формате."));

    private static MeasurementUnit? NormalizeUnit(string value)
    {
        string normalized = TextNormalizer.NormalizeSearchText(value);

        return normalized switch
        {
            "Г" or "G" or "GRAM" or "GRAMS" or "GRAMM" or "GRAMMS" or "GRAMME" => MeasurementUnit.Gram,
            "КГ" or "KG" or "KILOGRAM" or "KILOGRAMS" => MeasurementUnit.Kilogram,
            "МЛ" or "ML" or "MILLILITER" or "MILLILITERS" => MeasurementUnit.Milliliter,
            "Л" or "L" or "LITER" or "LITERS" => MeasurementUnit.Liter,
            "ШТ" or "PIECE" or "PIECES" => MeasurementUnit.Piece,
            "Ч. Л." or "Ч Л" or "TEASPOON" or "TEASPOONS" => MeasurementUnit.Teaspoon,
            "СТ. Л." or "СТ Л" or "TABLESPOON" or "TABLESPOONS" => MeasurementUnit.Tablespoon,
            "ЩЕПОТКА" or "PINCH" => MeasurementUnit.Pinch,
            "УП" or "УП." or "PACK" or "PACKAGE" => MeasurementUnit.Pack,
            "СТАКАН" or "СТАКАНА" or "СТАКАНОВ" or "GLASS" or "GLASSES" => MeasurementUnit.Glass,
            "ЧАШКА" or "ЧАШКИ" or "ЧАШЕК" or "CUP" or "CUPS" => MeasurementUnit.Cup,
            "ДЕС. Л." or "ДЕС Л" or "ДЕСЕРТНАЯ ЛОЖКА" or "DESSERTSPOON" => MeasurementUnit.Dessertspoon,
            "ЗУБЧИК" or "ЗУБЧИКА" or "ЗУБЧИКОВ" or "CLOVE" or "CLOVES" => MeasurementUnit.Clove,
            "ПУЧОК" or "ПУЧКА" or "ПУЧКОВ" or "BUNCH" or "BUNCHES" => MeasurementUnit.Bunch,
            "ВЕТОЧКА" or "ВЕТОЧКИ" or "ВЕТОЧЕК" or "SPRIG" or "SPRIGS" => MeasurementUnit.Sprig,
            "ГОЛОВКА" or "ГОЛОВКИ" or "ГОЛОВОК" or "HEAD" or "HEADS" => MeasurementUnit.Head,
            "СТЕБЕЛЬ" or "СТЕБЛЯ" or "СТЕБЛЕЙ" or "STALK" or "STALKS" => MeasurementUnit.Stalk,
            "ЛОМТИК" or "ЛОМТИКА" or "ЛОМТИКОВ" or "SLICE" or "SLICES" => MeasurementUnit.Slice,
            "ЛИСТ" or "ЛИСТА" or "ЛИСТОВ" or "SHEET" or "SHEETS" => MeasurementUnit.Sheet,
            "ГОРСТЬ" or "ГОРСТИ" or "HANDFUL" or "HANDFULS" => MeasurementUnit.Handful,
            "КАПЛЯ" or "КАПЛИ" or "КАПЕЛЬ" or "DROP" or "DROPS" => MeasurementUnit.Drop,
            "БАНКА" or "БАНКИ" or "БАНОК" or "CAN" or "CANS" => MeasurementUnit.Can,
            "БАНОЧКА" or "БАНОЧКИ" or "JAR" or "JARS" => MeasurementUnit.Jar,
            "БУТЫЛКА" or "БУТЫЛКИ" or "БУТЫЛОК" or "BOTTLE" or "BOTTLES" => MeasurementUnit.Bottle,
            "ПАКЕТИК" or "ПАКЕТИКА" or "ПАКЕТИКОВ" or "SACHET" or "SACHETS" => MeasurementUnit.Sachet,
            "КУБИК" or "КУБИКА" or "КУБИКОВ" or "CUBE" or "CUBES" => MeasurementUnit.Cube,
            "ПО ВКУСУ" or "TOTASTE" or "TO TASTE" => MeasurementUnit.ToTaste,
            "UNKNOWN" or "НЕИЗВЕСТНО" => MeasurementUnit.Unknown,
            _ => Enum.TryParse(value, ignoreCase: true, out MeasurementUnit unit) ? unit : null
        };
    }
}

internal sealed record RecipeDraft(
    RecipeTitle Title,
    string? Description,
    Servings Servings,
    RecipeCategory Category,
    RecipeVisibility Visibility,
    int? TotalTimeMinutes,
    int? ActiveTimeMinutes,
    Uri? SourceUrl,
    IReadOnlyCollection<RecipeIngredient> Ingredients,
    IReadOnlyCollection<PreparationStep> Steps,
    IReadOnlyCollection<string> Tags);
