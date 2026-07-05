#pragma warning disable OPENAI001 // Responses API помечен experimental в официальном SDK OpenAI 2.11.0.

using System.Text.Json;
using MenuMate.Contracts.Recipes;
using MenuMate.Modules.RecipeImports.Application.Extraction;
using OpenAI.Responses;

namespace MenuMate.Modules.RecipeImports.Infrastructure.OpenAI;

internal sealed class OpenAiRecipeImageExtractor(
    ResponsesClient client,
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
            List<ResponseContentPart> contentParts =
            [
                ResponseContentPart.CreateInputTextPart(
                    "Извлеки один рецепт из всех приложенных изображений. Изображения идут в порядке загрузки и могут быть частями одного рецепта. " +
                    "Сохрани язык исходного рецепта. Не выдумывай отсутствующие данные. Не включай порядковые номера, слово «Шаг» или «Step» в text шагов приготовления. " +
                    "Неизвестные единицы указывай как Unknown, неизвестные категории как Other. Количество порций используй 1, только если оно не указано. " +
                    "Верни весь читаемый текст изображений в extractedText. В warnings добавляй только действительно спорные места, которые пользователь может проверить или исправить. " +
                    "Каждое замечание должно быть коротким, понятным пользователю и написанным по-русски: укажи, что именно проверить и почему. " +
                    "Не упоминай JSON, схему, названия полей, enum, null, Unknown, Other, confidence, модель, технические ограничения и внутренние решения.")
            ];
            contentParts.AddRange(images.Select(image =>
                ResponseContentPart.CreateInputImagePart(
                    new BinaryData(image.Content, image.ContentType),
                    ResponseImageDetailLevel.High)));

            CreateResponseOptions request = new(
                options.Model,
                [
                    ResponseItem.CreateUserMessageItem(contentParts)
                ])
            {
                Instructions =
                    "Ты извлекаешь структурированные данные рецепта из пользовательских изображений.",
                StoredOutputEnabled = false,
                TextOptions = new ResponseTextOptions
                {
                    TextFormat = ResponseTextFormat.CreateJsonSchemaFormat(
                        "menu_mate_recipe_import",
                        RecipeSchema,
                        "Черновик рецепта и понятные пользователю замечания для проверки.",
                        jsonSchemaIsStrict: true)
                }
            };

            ResponseResult response = await client.CreateResponseAsync(request, cancellationToken);
            string json = response.GetOutputText();
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
