using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuMate.Modules.Recipes.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class AddRecipeImageAttribution : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.AddColumn<string>(
            name: "author_name",
            schema: "recipes",
            table: "recipe_images",
            type: "character varying(500)",
            maxLength: 500,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "license_name",
            schema: "recipes",
            table: "recipe_images",
            type: "character varying(200)",
            maxLength: 200,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "license_url",
            schema: "recipes",
            table: "recipe_images",
            type: "character varying(2048)",
            maxLength: 2048,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "source_url",
            schema: "recipes",
            table: "recipe_images",
            type: "character varying(2048)",
            maxLength: 2048,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.DropColumn(
            name: "author_name",
            schema: "recipes",
            table: "recipe_images");

        migrationBuilder.DropColumn(
            name: "license_name",
            schema: "recipes",
            table: "recipe_images");

        migrationBuilder.DropColumn(
            name: "license_url",
            schema: "recipes",
            table: "recipe_images");

        migrationBuilder.DropColumn(
            name: "source_url",
            schema: "recipes",
            table: "recipe_images");
    }
}
