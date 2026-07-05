using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuMate.DataImporter.Infrastructure.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    private static readonly string[] SourceExternalIdColumns = ["source", "external_id"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.EnsureSchema(
            name: "data_import");

        migrationBuilder.CreateTable(
            name: "import_items",
            schema: "data_import",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                external_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                source_revision_id = table.Column<long>(type: "bigint", nullable: false),
                source_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                content_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                recipe_id = table.Column<Guid>(type: "uuid", nullable: true),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                last_error = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_import_items", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_import_items_recipe_id",
            schema: "data_import",
            table: "import_items",
            column: "recipe_id");

        migrationBuilder.CreateIndex(
            name: "ix_import_items_source_external_id",
            schema: "data_import",
            table: "import_items",
            columns: SourceExternalIdColumns,
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.DropTable(
            name: "import_items",
            schema: "data_import");
    }
}
