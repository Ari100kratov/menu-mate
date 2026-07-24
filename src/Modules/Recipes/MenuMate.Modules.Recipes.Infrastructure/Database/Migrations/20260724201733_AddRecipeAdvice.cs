using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuMate.Modules.Recipes.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class AddRecipeAdvice : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "advice",
            schema: "recipes",
            table: "recipes",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "advice",
            schema: "recipes",
            table: "recipe_revisions",
            type: "text",
            nullable: true);

        migrationBuilder.Sql(
            """
            UPDATE recipes.recipes
            SET advice = description,
                description = NULL
            WHERE description IS NOT NULL;

            UPDATE recipes.recipe_revisions
            SET advice = description,
                description = NULL
            WHERE description IS NOT NULL;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE recipes.recipes
            SET description = advice
            WHERE description IS NULL
              AND advice IS NOT NULL;

            UPDATE recipes.recipe_revisions
            SET description = advice
            WHERE description IS NULL
              AND advice IS NOT NULL;
            """);

        migrationBuilder.DropColumn(
            name: "advice",
            schema: "recipes",
            table: "recipes");

        migrationBuilder.DropColumn(
            name: "advice",
            schema: "recipes",
            table: "recipe_revisions");
    }
}
