using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuMate.Modules.Recipes.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class InitialRecipes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "recipes");

        migrationBuilder.CreateTable(
            name: "recipes",
            schema: "recipes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                owner_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                servings = table.Column<int>(type: "integer", nullable: false),
                is_favorite = table.Column<bool>(type: "boolean", nullable: false),
                source_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_recipes", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "preparation_steps",
            schema: "recipes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                recipe_id = table.Column<Guid>(type: "uuid", nullable: false),
                number = table.Column<int>(type: "integer", nullable: false),
                text = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_preparation_steps", x => x.id);
                table.ForeignKey(
                    name: "fk_preparation_steps_recipes_recipe_id",
                    column: x => x.recipe_id,
                    principalSchema: "recipes",
                    principalTable: "recipes",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "recipe_ingredients",
            schema: "recipes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                recipe_id = table.Column<Guid>(type: "uuid", nullable: false),
                order = table.Column<int>(type: "integer", nullable: false),
                product_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                normalized_product_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                amount = table.Column<decimal>(type: "numeric", nullable: true),
                unit = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                quantity_kind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                is_optional = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_recipe_ingredients", x => x.id);
                table.ForeignKey(
                    name: "fk_recipe_ingredients_recipes_recipe_id",
                    column: x => x.recipe_id,
                    principalSchema: "recipes",
                    principalTable: "recipes",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "recipe_tags",
            schema: "recipes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                recipe_id = table.Column<Guid>(type: "uuid", nullable: false),
                value = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                normalized_value = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_recipe_tags", x => x.id);
                table.ForeignKey(
                    name: "fk_recipe_tags_recipes_recipe_id",
                    column: x => x.recipe_id,
                    principalSchema: "recipes",
                    principalTable: "recipes",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_preparation_steps_recipe_id_number",
            schema: "recipes",
            table: "preparation_steps",
            columns: ["recipe_id", "number"],
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_recipe_ingredients_normalized_product_name",
            schema: "recipes",
            table: "recipe_ingredients",
            column: "normalized_product_name");

        migrationBuilder.CreateIndex(
            name: "ix_recipe_ingredients_recipe_id",
            schema: "recipes",
            table: "recipe_ingredients",
            column: "recipe_id");

        migrationBuilder.CreateIndex(
            name: "ix_recipe_tags_normalized_value",
            schema: "recipes",
            table: "recipe_tags",
            column: "normalized_value");

        migrationBuilder.CreateIndex(
            name: "ix_recipe_tags_recipe_id",
            schema: "recipes",
            table: "recipe_tags",
            column: "recipe_id");

        migrationBuilder.CreateIndex(
            name: "ix_recipes_is_deleted",
            schema: "recipes",
            table: "recipes",
            column: "is_deleted");

        migrationBuilder.CreateIndex(
            name: "ix_recipes_is_favorite",
            schema: "recipes",
            table: "recipes",
            column: "is_favorite");

        migrationBuilder.CreateIndex(
            name: "ix_recipes_owner_user_id",
            schema: "recipes",
            table: "recipes",
            column: "owner_user_id");

        migrationBuilder.CreateIndex(
            name: "ix_recipes_title",
            schema: "recipes",
            table: "recipes",
            column: "title");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "preparation_steps",
            schema: "recipes");

        migrationBuilder.DropTable(
            name: "recipe_ingredients",
            schema: "recipes");

        migrationBuilder.DropTable(
            name: "recipe_tags",
            schema: "recipes");

        migrationBuilder.DropTable(
            name: "recipes",
            schema: "recipes");
    }
}
