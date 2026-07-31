using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuMate.Modules.Auth.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class AddUserPreferences : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "show_shopping_list_preview",
            schema: "auth",
            table: "users",
            type: "boolean",
            nullable: false,
            defaultValue: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "show_shopping_list_preview",
            schema: "auth",
            table: "users");
    }
}
