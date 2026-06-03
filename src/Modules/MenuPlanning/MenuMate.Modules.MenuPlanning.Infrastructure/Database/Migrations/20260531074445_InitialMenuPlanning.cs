using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuMate.Modules.MenuPlanning.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class InitialMenuPlanning : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "menu_planning");

        migrationBuilder.CreateTable(
            name: "menu_plans",
            schema: "menu_planning",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                owner_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                start_date = table.Column<DateOnly>(type: "date", nullable: false),
                end_date = table.Column<DateOnly>(type: "date", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_menu_plans", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "menu_plan_items",
            schema: "menu_planning",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                menu_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                date = table.Column<DateOnly>(type: "date", nullable: false),
                meal_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                servings = table.Column<int>(type: "integer", nullable: false),
                recipe_id = table.Column<Guid>(type: "uuid", nullable: true),
                recipe_title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_menu_plan_items", x => x.id);
                table.ForeignKey(
                    name: "fk_menu_plan_items_menu_plans_menu_plan_id",
                    column: x => x.menu_plan_id,
                    principalSchema: "menu_planning",
                    principalTable: "menu_plans",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_menu_plan_items_menu_plan_id_date_meal_type",
            schema: "menu_planning",
            table: "menu_plan_items",
            columns: ["menu_plan_id", "date", "meal_type"]);

        migrationBuilder.CreateIndex(
            name: "ix_menu_plan_items_recipe_id",
            schema: "menu_planning",
            table: "menu_plan_items",
            column: "recipe_id");

        migrationBuilder.CreateIndex(
            name: "ix_menu_plans_end_date",
            schema: "menu_planning",
            table: "menu_plans",
            column: "end_date");

        migrationBuilder.CreateIndex(
            name: "ix_menu_plans_owner_user_id",
            schema: "menu_planning",
            table: "menu_plans",
            column: "owner_user_id");

        migrationBuilder.CreateIndex(
            name: "ix_menu_plans_start_date",
            schema: "menu_planning",
            table: "menu_plans",
            column: "start_date");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "menu_plan_items",
            schema: "menu_planning");

        migrationBuilder.DropTable(
            name: "menu_plans",
            schema: "menu_planning");
    }
}
