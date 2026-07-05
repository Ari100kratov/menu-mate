using MenuMate.Modules.ShoppingLists.Domain.Enums;

namespace MenuMate.Modules.ShoppingLists.Domain.Services;

/// <summary>
/// Отображаемые названия категорий продуктов.
/// </summary>
public static class ShoppingProductCategoryNames
{
    /// <summary>
    /// Возвращает русское название категории.
    /// </summary>
    public static string GetDisplayName(ShoppingProductCategory category) =>
        category switch
        {
            ShoppingProductCategory.Produce => "Овощи и фрукты",
            ShoppingProductCategory.Dairy => "Молочные продукты",
            ShoppingProductCategory.MeatAndPoultry => "Мясо и птица",
            ShoppingProductCategory.FishAndSeafood => "Рыба и морепродукты",
            ShoppingProductCategory.Grocery => "Бакалея",
            ShoppingProductCategory.GrainsAndPasta => "Крупы и макароны",
            ShoppingProductCategory.Spices => "Специи",
            ShoppingProductCategory.Bakery => "Хлеб и выпечка",
            ShoppingProductCategory.Drinks => "Напитки",
            ShoppingProductCategory.Frozen => "Заморозка",
            ShoppingProductCategory.Eggs => "Яйца",
            ShoppingProductCategory.OilsAndSauces => "Масла и соусы",
            ShoppingProductCategory.Legumes => "Бобовые",
            ShoppingProductCategory.NutsAndSeeds => "Орехи и семена",
            ShoppingProductCategory.CannedAndPreserved => "Консервы и заготовки",
            ShoppingProductCategory.SweetsAndConfectionery => "Сладости",
            ShoppingProductCategory.HerbsAndGreens => "Зелень и травы",
            _ => "Прочее"
        };
}
