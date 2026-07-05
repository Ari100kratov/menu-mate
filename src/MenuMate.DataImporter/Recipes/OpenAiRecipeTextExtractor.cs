#pragma warning disable OPENAI001 // Responses API остается экспериментальным в официальном SDK OpenAI 2.11.0.

using System.Text.Json;
using MenuMate.Contracts.Recipes;
using MenuMate.DataImporter.Wikibooks;
using Microsoft.Extensions.Configuration;
using OpenAI.Responses;

namespace MenuMate.DataImporter.Recipes;

internal sealed class OpenAiRecipeTextExtractor(ResponsesClient client, IConfiguration configuration)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly BinaryData RecipeSchema = BinaryData.FromString(
        """
        {
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
                  "unit": { "type": "string", "enum": ["Gram","Kilogram","Milliliter","Liter","Piece","Teaspoon","Tablespoon","Pinch","Pack","ToTaste","Glass","Cup","Dessertspoon","Clove","Bunch","Sprig","Head","Stalk","Slice","Sheet","Handful","Drop","Can","Jar","Bottle","Sachet","Cube"] },
                  "category": { "type": "string", "enum": ["Produce","Dairy","MeatAndPoultry","FishAndSeafood","Grocery","GrainsAndPasta","Spices","Bakery","Drinks","Frozen","Eggs","OilsAndSauces","Legumes","NutsAndSeeds","CannedAndPreserved","SweetsAndConfectionery","HerbsAndGreens"] },
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
        }
        """);

    public async Task<CreateRecipeRequest?> ExtractAsync(WikibooksPage page, CancellationToken cancellationToken)
    {
        string? apiKey = configuration["OpenAI:ApiKey"] ?? configuration["OPENAI_API_KEY"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return null;
        }

        string model = configuration["OpenAI:Model"] ?? "gpt-5.4-mini";
        CreateResponseOptions request = new(
            model,
            [
                ResponseItem.CreateUserMessageItem(
                    """
                    Преобразуй русскоязычную страницу Wikibooks в один рецепт MenuMate.
                    Если страница содержит несколько вариантов рецепта, выбери первый полный и связный вариант.
                    Не смешивай ингредиенты и шаги разных вариантов. Не выдумывай отсутствующие ингредиенты.
                    Удали порядковые номера из текста шагов.
                    Если количество порций не указано, используй 1.
                    Не используй Unknown для единицы измерения и Other для категории продукта.
                    Для счетного продукта без указанной единицы используй Piece.
                    Если количество не указано, используй amount=null и unit=ToTaste.
                    Если точную категорию продукта определить нельзя, используй Grocery.
                    Исходный URL рецепта:
                    """ +
                    $"\n{page.SourceUrl}\n\nРазметка страницы:\n{page.Wikitext}")
            ])
        {
            Instructions = "Ты структурируешь русскоязычные рецепты из Wikibooks.",
            StoredOutputEnabled = false,
            TextOptions = new ResponseTextOptions
            {
                TextFormat = ResponseTextFormat.CreateJsonSchemaFormat(
                    "menu_mate_wikibooks_recipe",
                    RecipeSchema,
                    "Структурированный русскоязычный рецепт.",
                    jsonSchemaIsStrict: true)
            }
        };

        ResponseResult response = await client.CreateResponseAsync(request, cancellationToken);
        CreateRecipeRequest? recipe = JsonSerializer.Deserialize<CreateRecipeRequest>(response.GetOutputText(), JsonOptions);
        if (recipe is null)
        {
            return null;
        }

        return recipe with
        {
            SourceUrl = page.SourceUrl,
            Visibility = "Public",
            Steps = recipe.Steps
                .Select(step => new PreparationStepRequest(RecipeTextNormalizer.NormalizeStep(step.Text)))
                .Where(step => !string.IsNullOrWhiteSpace(step.Text))
                .ToArray()
        };
    }
}
