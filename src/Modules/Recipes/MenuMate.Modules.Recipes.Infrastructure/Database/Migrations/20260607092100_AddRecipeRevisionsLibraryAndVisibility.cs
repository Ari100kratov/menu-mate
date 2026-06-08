using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuMate.Modules.Recipes.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class AddRecipeRevisionsLibraryAndVisibility : Migration
{
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_recipes_is_favorite",
                schema: "recipes",
                table: "recipes");

            migrationBuilder.DropColumn(
                name: "is_favorite",
                schema: "recipes",
                table: "recipes");

            migrationBuilder.AddColumn<Guid>(
                name: "current_revision_id",
                schema: "recipes",
                table: "recipes",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<int>(
                name: "revision_number",
                schema: "recipes",
                table: "recipes",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<Guid>(
                name: "source_recipe_id",
                schema: "recipes",
                table: "recipes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "source_revision_id",
                schema: "recipes",
                table: "recipes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "visibility",
                schema: "recipes",
                table: "recipes",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Private");

            migrationBuilder.CreateTable(
                name: "recipe_library_entries",
                schema: "recipes",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipe_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_favorite = table.Column<bool>(type: "boolean", nullable: false),
                    saved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recipe_library_entries", x => new { x.user_id, x.recipe_id });
                    table.ForeignKey(
                        name: "fk_recipe_library_entries_recipes_recipe_id",
                        column: x => x.recipe_id,
                        principalSchema: "recipes",
                        principalTable: "recipes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recipe_revisions",
                schema: "recipes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipe_id = table.Column<Guid>(type: "uuid", nullable: false),
                    revision_number = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    servings = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    total_time_minutes = table.Column<int>(type: "integer", nullable: true),
                    active_time_minutes = table.Column<int>(type: "integer", nullable: true),
                    source_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recipe_revisions", x => x.id);
                    table.ForeignKey(
                        name: "fk_recipe_revisions_recipes_recipe_id",
                        column: x => x.recipe_id,
                        principalSchema: "recipes",
                        principalTable: "recipes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recipe_revision_ingredients",
                schema: "recipes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipe_revision_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    ingredient_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("pk_recipe_revision_ingredients", x => x.id);
                    table.ForeignKey(
                        name: "fk_recipe_revision_ingredients_recipe_revisions_recipe_revisio",
                        column: x => x.recipe_revision_id,
                        principalSchema: "recipes",
                        principalTable: "recipe_revisions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recipe_revision_steps",
                schema: "recipes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipe_revision_id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<int>(type: "integer", nullable: false),
                    text = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recipe_revision_steps", x => x.id);
                    table.ForeignKey(
                        name: "fk_recipe_revision_steps_recipe_revisions_recipe_revision_id",
                        column: x => x.recipe_revision_id,
                        principalSchema: "recipes",
                        principalTable: "recipe_revisions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recipe_revision_tags",
                schema: "recipes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipe_revision_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    normalized_value = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recipe_revision_tags", x => x.id);
                    table.ForeignKey(
                        name: "fk_recipe_revision_tags_recipe_revisions_recipe_revision_id",
                        column: x => x.recipe_revision_id,
                        principalSchema: "recipes",
                        principalTable: "recipe_revisions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_recipes_current_revision_id",
                schema: "recipes",
                table: "recipes",
                column: "current_revision_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_recipes_source_recipe_id",
                schema: "recipes",
                table: "recipes",
                column: "source_recipe_id");

            migrationBuilder.CreateIndex(
                name: "ix_recipes_visibility",
                schema: "recipes",
                table: "recipes",
                column: "visibility");

            migrationBuilder.CreateIndex(
                name: "ix_recipe_library_entries_recipe_id",
                schema: "recipes",
                table: "recipe_library_entries",
                column: "recipe_id");

            migrationBuilder.CreateIndex(
                name: "ix_recipe_library_entries_user_id_is_favorite",
                schema: "recipes",
                table: "recipe_library_entries",
                columns: ["user_id", "is_favorite"]);

            migrationBuilder.CreateIndex(
                name: "ix_recipe_revision_ingredients_recipe_revision_id_order",
                schema: "recipes",
                table: "recipe_revision_ingredients",
                columns: ["recipe_revision_id", "order"],
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_recipe_revision_steps_recipe_revision_id_number",
                schema: "recipes",
                table: "recipe_revision_steps",
                columns: ["recipe_revision_id", "number"],
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_recipe_revision_tags_recipe_revision_id_normalized_value",
                schema: "recipes",
                table: "recipe_revision_tags",
                columns: ["recipe_revision_id", "normalized_value"],
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_recipe_revisions_recipe_id_revision_number",
                schema: "recipes",
                table: "recipe_revisions",
                columns: ["recipe_id", "revision_number"],
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recipe_library_entries",
                schema: "recipes");

            migrationBuilder.DropTable(
                name: "recipe_revision_ingredients",
                schema: "recipes");

            migrationBuilder.DropTable(
                name: "recipe_revision_steps",
                schema: "recipes");

            migrationBuilder.DropTable(
                name: "recipe_revision_tags",
                schema: "recipes");

            migrationBuilder.DropTable(
                name: "recipe_revisions",
                schema: "recipes");

            migrationBuilder.DropIndex(
                name: "ix_recipes_current_revision_id",
                schema: "recipes",
                table: "recipes");

            migrationBuilder.DropIndex(
                name: "ix_recipes_source_recipe_id",
                schema: "recipes",
                table: "recipes");

            migrationBuilder.DropIndex(
                name: "ix_recipes_visibility",
                schema: "recipes",
                table: "recipes");

            migrationBuilder.DropColumn(
                name: "current_revision_id",
                schema: "recipes",
                table: "recipes");

            migrationBuilder.DropColumn(
                name: "revision_number",
                schema: "recipes",
                table: "recipes");

            migrationBuilder.DropColumn(
                name: "source_recipe_id",
                schema: "recipes",
                table: "recipes");

            migrationBuilder.DropColumn(
                name: "source_revision_id",
                schema: "recipes",
                table: "recipes");

            migrationBuilder.DropColumn(
                name: "visibility",
                schema: "recipes",
                table: "recipes");

            migrationBuilder.AddColumn<bool>(
                name: "is_favorite",
                schema: "recipes",
                table: "recipes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_recipes_is_favorite",
                schema: "recipes",
                table: "recipes",
                column: "is_favorite");
        }
}
