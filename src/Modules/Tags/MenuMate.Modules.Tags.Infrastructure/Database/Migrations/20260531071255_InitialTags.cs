using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuMate.Modules.Tags.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class InitialTags : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "tags");

        migrationBuilder.CreateTable(
            name: "tags",
            schema: "tags",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                normalized_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tags", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_tags_normalized_name",
            schema: "tags",
            table: "tags",
            column: "normalized_name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_tags_status",
            schema: "tags",
            table: "tags",
            column: "status");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "tags",
            schema: "tags");
    }
}
