using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuMate.Modules.ShoppingLists.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class AddProductCatalogReference : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "product_id",
            schema: "shopping_lists",
            table: "shopping_list_items",
            type: "uuid",
            nullable: false,
            defaultValue: Guid.Empty);

        migrationBuilder.CreateIndex(
            name: "ix_shopping_list_items_product_id",
            schema: "shopping_lists",
            table: "shopping_list_items",
            column: "product_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_shopping_list_items_product_id",
            schema: "shopping_lists",
            table: "shopping_list_items");

        migrationBuilder.DropColumn(
            name: "product_id",
            schema: "shopping_lists",
            table: "shopping_list_items");
    }
}
