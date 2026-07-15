using System.Text.Json;
using MenuMate.Contracts.Recipes;
using MenuMate.Modules.RecipeImports.Application.Extraction;
using OpenAI.Chat;

namespace MenuMate.Modules.RecipeImports.Infrastructure.OpenAI;

internal sealed class OpenAiRecipeImageExtractor(
    ChatClient client,
    OpenAiRecipeImageExtractorOptions options)
    : IRecipeImageExtractor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly BinaryData RecipeSchema = CreateRecipeSchema();

    public async Task<RecipeImageExtractionResult> ExtractAsync(
        IReadOnlyCollection<RecipeImageInput> images,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey) ||
            string.Equals(options.ApiKey, "not-configured", StringComparison.Ordinal))
        {
            throw new RecipeImageExtractionException(
                "Серверный ключ OpenAI не настроен.");
        }

        try
        {
            List<ChatMessageContentPart> contentParts =
            [
                ChatMessageContentPart.CreateTextPart(
                    """
                    Задача:
                    - Извлеки один рецепт из всех приложенных изображений. Изображения могут быть частями одного рецепта и идти в произвольном порядке.
                    - Распознавай дубли и перекрывающиеся фрагменты страниц: используй их для сверки, но не повторяй ингредиенты, шаги или текст.
                    - Сохрани язык исходного рецепта и не выдумывай отсутствующие данные.
                    - В тексте шагов не оставляй порядковые номера, слова «Шаг» или «Step».
                    - Для каждого элемента recipe.ingredients верни в ingredientSourceTexts точную исходную строку ингредиента. Порядок и количество строк должны совпадать с recipe.ingredients. Если одна строка разделена на два продукта, повтори исходную строку для обоих.
                    """),
                ChatMessageContentPart.CreateTextPart(
                    """
                    Ингредиенты — обязательные правила:
                    - Один самостоятельный продукт = один элемент. «Соль и перец» всегда разделяй на «соль» и «перец», даже если они написаны в одной строке.
                    - Диапазон количества относится к одному элементу: «1–2 болгарских перца» — это один продукт с диапазоном, а не два одинаковых продукта по одной штуке. Никогда не создавай отдельные элементы для нижней и верхней границ диапазона.
                    - В productName указывай словарное название продукта в именительном падеже, без количества и единицы. Например: «лук», а не «лука»; «куриное яйцо», а не «яйца».
                    - Значимая характеристика продукта остаётся частью productName. Процент жирности или концентрации нельзя превращать в количество или комментарий: «сливки 33% — 80 г» => productName="сливки 33%", amount=80, unit="Gram". Так же обрабатывай молоко, сметану, кефир, йогурт, творог, сыр, уксус, шоколад и подобные продукты.
                    - Сначала найди все числа и подпиши их смысл: процент относится к продукту; число перед единицей — к количеству; число порций — только к servings. Не подменяй одно другим.
                    - Если дана точная масса или объём вместе со штуками/упаковкой, масса или объём приоритетнее: «2–3 кабачка (380 г)» => amount=380, unit="Gram", comment="2–3 шт.".
                    - Если дан диапазон без более точной массы/объёма, в amount верни нижнюю границу, сохрани правильную unit, а полный диапазон продублируй в comment: «2–3 персика» => amount=2, unit="Piece", comment="2–3 шт.". Никогда не превращай диапазон в ToTaste.
                    - Если единица указана без числа, считай количество равным 1: «пучок петрушки» => amount=1, unit="Bunch"; «щепотка соли» => amount=1, unit="Pinch".
                    - Различай Glass и Cup буквально: «стакан» = Glass, «чашка» = Cup. Не заменяй стакан чашкой.
                    - «зуб.», «зуб» и «зубчик» = Clove.
                    - ToTaste допустим только когда в исходной строке прямо написано «по вкусу» либо нет ни количества, ни единицы. Если есть число, диапазон или единица, ToTaste запрещён.
                    - Неизвестную единицу указывай как Unknown, неизвестную категорию — как Other.
                    - servings=1 используй только если число порций отсутствует.
                    """),
                ChatMessageContentPart.CreateTextPart(
                    $"""
                    Допустимые единицы и распознаваемые русские формы:
                    {RecipeMeasurementUnitVocabulary.CreatePromptReference()}
                    """),
                ChatMessageContentPart.CreateTextPart(
                    """
                    Категория блюда:
                    - Выбери одну наиболее точную категорию из допустимых значений схемы; категория описывает роль блюда, а не случайное упоминание в тексте.
                    - Spread — густая масса для намазывания на хлеб, тосты или крекеры: паштет, хумус, творожная или сырная намазка.
                    - Sauce — жидкий или полужидкий соус, которым дополняют другое блюдо. Appetizer — самостоятельная закуска, а не намазка.
                    - Baking — выпечка из теста; Dessert — сладкое блюдо, не обязательно выпечка. Breakfast выбирай только для блюда, предназначенного прежде всего для завтрака.
                    - Other выбирай только если ни одна другая категория не подходит.

                    Теги:
                    - Верни от 0 до 6 коротких тегов, только если они полезны для поиска.
                    - Подходят кухня, способ приготовления, тип питания, повод или практическая потребность. Основной ингредиент допустим, только если его нет в названии блюда.
                    - Не используй тип блюда и приём пищи («завтрак», «обед», «ужин», «суп», «выпечка», «салат», «десерт» и аналоги), название блюда, слова «еда» и «рецепт», повторы и слишком общие теги.

                    Результат и предупреждения:
                    - Верни весь читаемый текст изображений в extractedText.
                    - В warnings добавляй только действительно спорные места, которые пользователь может проверить или исправить.
                    - Каждое замечание должно быть коротким, понятным и написанным по-русски: укажи, что проверить и почему.
                    - Не упоминай JSON, схему, названия полей, enum, null, Unknown, Other, confidence, модель, технические ограничения и внутренние решения.
                    """)
            ];
            contentParts.AddRange(images.Select(image =>
                ChatMessageContentPart.CreateImagePart(
                    BinaryData.FromBytes(image.Content),
                    image.ContentType,
                    ChatImageDetailLevel.High)));

            ChatCompletionOptions request = new()
            {
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                    "menu_mate_recipe_import",
                    RecipeSchema,
                    "Черновик рецепта, исходные строки ингредиентов и понятные пользователю замечания для проверки.",
                    jsonSchemaIsStrict: true),
                StoredOutputEnabled = false
            };

            List<ChatMessage> messages =
            [
                new SystemChatMessage(
                    "Ты точно извлекаешь структурированные данные рецепта из пользовательских изображений и сохраняешь смысл каждого числа и единицы измерения."),
                new UserChatMessage(contentParts)
            ];

            ChatCompletion response = await client.CompleteChatAsync(messages, request, cancellationToken);
            string json = response.Content[0].Text;
            OpenAiExtractionPayload? payload = JsonSerializer.Deserialize<OpenAiExtractionPayload>(
                json,
                JsonOptions);
            if (payload?.Recipe is null)
            {
                throw new RecipeImageExtractionException(
                    "OpenAI вернул пустой или некорректный структурированный ответ.");
            }

            return new RecipeImageExtractionResult(
                RecipeImportTextNormalizer.Normalize(payload.Recipe, payload.IngredientSourceTexts),
                payload.ExtractedText ?? string.Empty,
                payload.Warnings ?? [],
                "OpenAI",
                options.Model,
                response.Id);
        }
        catch (RecipeImageExtractionException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new RecipeImageExtractionException(
                "Внешний сервис распознавания временно недоступен.",
                exception);
        }
    }

    private static BinaryData CreateRecipeSchema()
    {
        string measurementUnits = JsonSerializer.Serialize(
            RecipeMeasurementUnitVocabulary.All.Select(unit => unit.Value));
        string schema =
            """
            {
              "type": "object",
              "properties": {
                "recipe": {
                  "type": "object",
                  "properties": {
                    "title": { "type": "string" },
                    "description": { "type": ["string", "null"] },
                    "servings": { "type": "integer", "minimum": 1 },
                    "category": { "type": "string", "enum": ["Breakfast","Soup","MainCourse","SideDish","Salad","Appetizer","Dessert","Baking","Drink","Sauce","Spread","Other"] },
                    "visibility": { "type": "string", "enum": ["Public"] },
                    "totalTimeMinutes": { "type": ["integer", "null"], "minimum": 0 },
                    "activeTimeMinutes": { "type": ["integer", "null"], "minimum": 0 },
                    "sourceUrl": { "type": ["string", "null"] },
                    "ingredients": {
                      "type": "array",
                      "items": {
                        "type": "object",
                        "properties": {
                          "ingredientId": { "type": "null" },
                          "productName": { "type": "string" },
                          "amount": { "type": ["number", "null"] },
                          "unit": { "type": "string", "enum": __MEASUREMENT_UNITS__ },
                          "category": { "type": "string", "enum": ["Produce","Dairy","MeatAndPoultry","FishAndSeafood","Grocery","GrainsAndPasta","Spices","Bakery","Drinks","Frozen","Other","Eggs","OilsAndSauces","Legumes","NutsAndSeeds","CannedAndPreserved","SweetsAndConfectionery","HerbsAndGreens"] },
                          "comment": { "type": ["string", "null"] },
                          "isOptional": { "type": "boolean" }
                        },
                        "required": ["ingredientId","productName","amount","unit","category","comment","isOptional"],
                        "additionalProperties": false
                      }
                    },
                    "steps": {
                      "type": "array",
                      "items": {
                        "type": "object",
                        "properties": { "text": { "type": "string" } },
                        "required": ["text"],
                        "additionalProperties": false
                      }
                    },
                    "tags": { "type": "array", "items": { "type": "string" } }
                  },
                  "required": ["title","description","servings","category","visibility","totalTimeMinutes","activeTimeMinutes","sourceUrl","ingredients","steps","tags"],
                  "additionalProperties": false
                },
                "ingredientSourceTexts": {
                  "type": "array",
                  "items": { "type": "string" },
                  "description": "Точная исходная строка для каждого ингредиента в том же порядке."
                },
                "extractedText": { "type": "string" },
                "warnings": { "type": "array", "items": { "type": "string" } }
              },
              "required": ["recipe","ingredientSourceTexts","extractedText","warnings"],
              "additionalProperties": false
            }
            """
            .Replace("__MEASUREMENT_UNITS__", measurementUnits, StringComparison.Ordinal);

        return BinaryData.FromString(schema);
    }

    private sealed record OpenAiExtractionPayload(
        CreateRecipeRequest? Recipe,
        IReadOnlyList<string>? IngredientSourceTexts,
        string? ExtractedText,
        IReadOnlyCollection<string>? Warnings);
}
