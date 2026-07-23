using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuMate.Modules.ShoppingLists.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class MakeShoppingListSourcePeriodOptional : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateOnly>(
            name: "source_start_date",
            schema: "shopping_lists",
            table: "shopping_lists",
            type: "date",
            nullable: true,
            oldClrType: typeof(DateOnly),
            oldType: "date");

        migrationBuilder.AlterColumn<DateOnly>(
            name: "source_end_date",
            schema: "shopping_lists",
            table: "shopping_lists",
            type: "date",
            nullable: true,
            oldClrType: typeof(DateOnly),
            oldType: "date");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateOnly>(
            name: "source_start_date",
            schema: "shopping_lists",
            table: "shopping_lists",
            type: "date",
            nullable: false,
            defaultValue: new DateOnly(1, 1, 1),
            oldClrType: typeof(DateOnly),
            oldType: "date",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateOnly>(
            name: "source_end_date",
            schema: "shopping_lists",
            table: "shopping_lists",
            type: "date",
            nullable: false,
            defaultValue: new DateOnly(1, 1, 1),
            oldClrType: typeof(DateOnly),
            oldType: "date",
            oldNullable: true);
    }
}
