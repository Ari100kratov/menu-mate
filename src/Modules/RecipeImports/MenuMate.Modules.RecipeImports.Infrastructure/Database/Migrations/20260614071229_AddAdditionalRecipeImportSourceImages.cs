using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuMate.Modules.RecipeImports.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class AddAdditionalRecipeImportSourceImages : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "additional_source_images_json",
            schema: "imports",
            table: "recipe_import_drafts",
            type: "jsonb",
            nullable: false,
            defaultValue: "[]");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "additional_source_images_json",
            schema: "imports",
            table: "recipe_import_drafts");
    }
}
