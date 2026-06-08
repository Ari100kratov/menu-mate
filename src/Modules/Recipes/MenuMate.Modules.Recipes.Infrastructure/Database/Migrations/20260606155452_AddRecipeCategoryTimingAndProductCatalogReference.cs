using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuMate.Modules.Recipes.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class AddRecipeCategoryTimingAndProductCatalogReference : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "active_time_minutes",
            schema: "recipes",
            table: "recipes",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "category",
            schema: "recipes",
            table: "recipes",
            type: "character varying(64)",
            maxLength: 64,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<int>(
            name: "total_time_minutes",
            schema: "recipes",
            table: "recipes",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "ingredient_id",
            schema: "recipes",
            table: "recipe_ingredients",
            type: "uuid",
            nullable: false,
            defaultValue: Guid.Empty);

        migrationBuilder.CreateIndex(
            name: "ix_recipes_category",
            schema: "recipes",
            table: "recipes",
            column: "category");

        migrationBuilder.CreateIndex(
            name: "ix_recipe_ingredients_ingredient_id",
            schema: "recipes",
            table: "recipe_ingredients",
            column: "ingredient_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_recipes_category",
            schema: "recipes",
            table: "recipes");

        migrationBuilder.DropIndex(
            name: "ix_recipe_ingredients_ingredient_id",
            schema: "recipes",
            table: "recipe_ingredients");

        migrationBuilder.DropColumn(
            name: "active_time_minutes",
            schema: "recipes",
            table: "recipes");

        migrationBuilder.DropColumn(
            name: "category",
            schema: "recipes",
            table: "recipes");

        migrationBuilder.DropColumn(
            name: "total_time_minutes",
            schema: "recipes",
            table: "recipes");

        migrationBuilder.DropColumn(
            name: "ingredient_id",
            schema: "recipes",
            table: "recipe_ingredients");
    }
}
