using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuMate.Modules.ShoppingLists.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class ReadPinnedRecipeRevisions : Migration
{
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Read-model source tables are excluded from ShoppingLists migrations.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Read-model source tables are excluded from ShoppingLists migrations.
        }
}
