using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuMate.Modules.MenuPlanning.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class PinMenuItemsToRecipeRevisions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "recipe_revision_id",
            schema: "menu_planning",
            table: "menu_plan_items",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "ix_menu_plan_items_recipe_revision_id",
            schema: "menu_planning",
            table: "menu_plan_items",
            column: "recipe_revision_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_menu_plan_items_recipe_revision_id",
            schema: "menu_planning",
            table: "menu_plan_items");

        migrationBuilder.DropColumn(
            name: "recipe_revision_id",
            schema: "menu_planning",
            table: "menu_plan_items");
    }
}
