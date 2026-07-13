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
    private static readonly BinaryData RecipeSchema = BinaryData.FromString(
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
                "category": { "type": "string", "enum": ["Breakfast","Soup","MainCourse","SideDish","Salad","Appetizer","Dessert","Baking","Drink","Sauce","Other"] },
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
                      "unit": { "type": "string", "enum": ["Gram","Kilogram","Milliliter","Liter","Piece","Teaspoon","Tablespoon","Pinch","Pack","ToTaste","Unknown","Glass","Cup","Dessertspoon","Clove","Bunch","Sprig","Head","Stalk","Slice","Sheet","Handful","Drop","Can","Jar","Bottle","Sachet","Cube"] },
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
            "extractedText": { "type": "string" },
            "warnings": { "type": "array", "items": { "type": "string" } }
          },
          "required": ["recipe","extractedText","warnings"],
          "additionalProperties": false
        }
        """);

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
                    - Извлеки один рецепт из всех приложенных изображений. Они могут быть частями одного рецепта, загруженными в произвольном порядке.
                    - Распознавай дубли и перекрывающиеся фрагменты страниц: используй их для сверки, но не повторяй ингредиенты, шаги или текст в результате.
                    - Сохрани язык исходного рецепта и не выдумывай отсутствующие данные.
                    - В text шагов приготовления не включай порядковые номера, слово «Шаг» или «Step».
                    """),
                ChatMessageContentPart.CreateTextPart(
                    """
                    Ингредиенты и порции:
                    - Неизвестную единицу указывай как Unknown, неизвестную категорию — как Other.
                    - Если указано «щепотка», в том числе без числа, верни amount=1 и unit=Pinch; не добавляй «щепотка» в comment.
                    - Если для ингредиента не указаны ни количество, ни единица измерения, верни amount=null и unit=ToTaste.
                    - Используй количество порций 1 только если оно не указано.
                    """),
                ChatMessageContentPart.CreateTextPart(
                    """
                    Теги:
                    - Верни от 0 до 6 коротких тегов, только если они полезны для поиска.
                    - Подходят кухня (например, «азиатская кухня»), способ приготовления, тип питания («вегетарианское», «без мяса»), повод или практическая потребность.
                    - Основной ингредиент допустим, только если его нет в названии блюда.
                    - Не используй тип блюда и приём пищи: «завтрак», «обед», «ужин», «суп», «выпечка», «салат», «десерт» и их английские аналоги.
                    - Не добавляй название блюда, слова «еда» и «рецепт», повторы и слишком общие теги. Если полезных тегов нет, верни пустой список.
                    """),
                ChatMessageContentPart.CreateTextPart(
                    """
                    Результат и предупреждения:
                    - Верни весь читаемый текст изображений в extractedText.
                    - В warnings добавляй только действительно спорные места, которые пользователь может проверить или исправить.
                    - Каждое замечание должно быть коротким, понятным пользователю и написанным по-русски: укажи, что именно проверить и почему.
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
                    "Черновик рецепта и понятные пользователю замечания для проверки.",
                    jsonSchemaIsStrict: true),
                StoredOutputEnabled = false
            };

            List<ChatMessage> messages =
            [
                new SystemChatMessage(
                    "Ты извлекаешь структурированные данные рецепта из пользовательских изображений."),
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
                RecipeImportTextNormalizer.Normalize(payload.Recipe),
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

    private sealed record OpenAiExtractionPayload(
        CreateRecipeRequest? Recipe,
        string? ExtractedText,
        IReadOnlyCollection<string>? Warnings);
}
