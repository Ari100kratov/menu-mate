using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuMate.Modules.Recipes.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class AddRecipeImages : Migration
{
    private static readonly string[] RecipeScopeIndexColumns = ["recipe_id", "scope"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "recipe_images",
            schema: "recipes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                owner_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                recipe_id = table.Column<Guid>(type: "uuid", nullable: false),
                scope = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                step_number = table.Column<int>(type: "integer", nullable: true),
                bucket_name = table.Column<string>(type: "character varying(63)", maxLength: 63, nullable: false),
                object_key = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                size_bytes = table.Column<long>(type: "bigint", nullable: false),
                original_file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                alt_text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_recipe_images", x => x.id);
                table.CheckConstraint("ck_recipe_images_step_number_matches_scope", "(scope = 'Step' AND step_number IS NOT NULL)\nOR (scope = 'Cover' AND step_number IS NULL)");
                table.ForeignKey(
                    name: "fk_recipe_images_recipes_recipe_id",
                    column: x => x.recipe_id,
                    principalSchema: "recipes",
                    principalTable: "recipes",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_recipe_images_object_key",
            schema: "recipes",
            table: "recipe_images",
            column: "object_key",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_recipe_images_owner_user_id",
            schema: "recipes",
            table: "recipe_images",
            column: "owner_user_id");

        migrationBuilder.CreateIndex(
            name: "ix_recipe_images_recipe_id",
            schema: "recipes",
            table: "recipe_images",
            column: "recipe_id");

        migrationBuilder.CreateIndex(
            name: "ix_recipe_images_recipe_id_scope",
            schema: "recipes",
            table: "recipe_images",
            columns: RecipeScopeIndexColumns);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "recipe_images",
            schema: "recipes");
    }
}
