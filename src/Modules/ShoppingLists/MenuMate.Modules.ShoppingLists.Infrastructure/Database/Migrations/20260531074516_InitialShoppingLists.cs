using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuMate.Modules.ShoppingLists.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class InitialShoppingLists : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "shopping_lists");

        migrationBuilder.CreateTable(
            name: "shopping_lists",
            schema: "shopping_lists",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                owner_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                source_menu_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_shopping_lists", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "shopping_list_items",
            schema: "shopping_lists",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                shopping_list_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                normalized_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                amount = table.Column<decimal>(type: "numeric", nullable: true),
                unit = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                quantity_kind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                is_purchased = table.Column<bool>(type: "boolean", nullable: false),
                is_in_stock = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_shopping_list_items", x => x.id);
                table.ForeignKey(
                    name: "fk_shopping_list_items_shopping_lists_shopping_list_id",
                    column: x => x.shopping_list_id,
                    principalSchema: "shopping_lists",
                    principalTable: "shopping_lists",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_shopping_list_items_is_in_stock",
            schema: "shopping_lists",
            table: "shopping_list_items",
            column: "is_in_stock");

        migrationBuilder.CreateIndex(
            name: "ix_shopping_list_items_is_purchased",
            schema: "shopping_lists",
            table: "shopping_list_items",
            column: "is_purchased");

        migrationBuilder.CreateIndex(
            name: "ix_shopping_list_items_shopping_list_id_normalized_name",
            schema: "shopping_lists",
            table: "shopping_list_items",
            columns: ["shopping_list_id", "normalized_name"]);

        migrationBuilder.CreateIndex(
            name: "ix_shopping_lists_created_at",
            schema: "shopping_lists",
            table: "shopping_lists",
            column: "created_at");

        migrationBuilder.CreateIndex(
            name: "ix_shopping_lists_owner_user_id",
            schema: "shopping_lists",
            table: "shopping_lists",
            column: "owner_user_id");

        migrationBuilder.CreateIndex(
            name: "ix_shopping_lists_source_menu_plan_id",
            schema: "shopping_lists",
            table: "shopping_lists",
            column: "source_menu_plan_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "shopping_list_items",
            schema: "shopping_lists");

        migrationBuilder.DropTable(
            name: "shopping_lists",
            schema: "shopping_lists");
    }
}
