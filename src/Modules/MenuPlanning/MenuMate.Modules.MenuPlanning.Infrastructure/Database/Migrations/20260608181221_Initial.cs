using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuMate.Modules.MenuPlanning.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "menu_planning");

        migrationBuilder.CreateTable(
            name: "meal_slots",
            schema: "menu_planning",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                owner_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                sort_order = table.Column<int>(type: "integer", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_meal_slots", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "menu_calendar_items",
            schema: "menu_planning",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                owner_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                date = table.Column<DateOnly>(type: "date", nullable: false),
                meal_slot_id = table.Column<Guid>(type: "uuid", nullable: false),
                position = table.Column<int>(type: "integer", nullable: false),
                servings = table.Column<int>(type: "integer", nullable: false),
                recipe_id = table.Column<Guid>(type: "uuid", nullable: true),
                recipe_revision_id = table.Column<Guid>(type: "uuid", nullable: true),
                recipe_title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_menu_calendar_items", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_meal_slots_owner_user_id",
            schema: "menu_planning",
            table: "meal_slots",
            column: "owner_user_id");

        migrationBuilder.CreateIndex(
            name: "ix_meal_slots_owner_user_id_sort_order",
            schema: "menu_planning",
            table: "meal_slots",
            columns: ["owner_user_id", "sort_order"]);

        migrationBuilder.CreateIndex(
            name: "ix_menu_calendar_items_owner_user_id_date",
            schema: "menu_planning",
            table: "menu_calendar_items",
            columns: ["owner_user_id", "date"]);

        migrationBuilder.CreateIndex(
            name: "ix_menu_calendar_items_owner_user_id_date_meal_slot_id_position",
            schema: "menu_planning",
            table: "menu_calendar_items",
            columns: ["owner_user_id", "date", "meal_slot_id", "position"]);

        migrationBuilder.CreateIndex(
            name: "ix_menu_calendar_items_recipe_id",
            schema: "menu_planning",
            table: "menu_calendar_items",
            column: "recipe_id");

        migrationBuilder.CreateIndex(
            name: "ix_menu_calendar_items_recipe_revision_id",
            schema: "menu_planning",
            table: "menu_calendar_items",
            column: "recipe_revision_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "meal_slots",
            schema: "menu_planning");

        migrationBuilder.DropTable(
            name: "menu_calendar_items",
            schema: "menu_planning");
    }
}
