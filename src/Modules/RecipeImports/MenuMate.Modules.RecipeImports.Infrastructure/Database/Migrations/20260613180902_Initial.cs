using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuMate.Modules.RecipeImports.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "imports");

        migrationBuilder.CreateTable(
            name: "recipe_import_drafts",
            schema: "imports",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                owner_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                target_recipe_id = table.Column<Guid>(type: "uuid", nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                title = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                recipe_json = table.Column<string>(type: "jsonb", nullable: false),
                evidence_json = table.Column<string>(type: "jsonb", nullable: false),
                bucket_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                object_key = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                content_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                size_bytes = table.Column<long>(type: "bigint", nullable: false),
                file_name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_recipe_id = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_recipe_import_drafts", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_recipe_import_drafts_created_recipe_id",
            schema: "imports",
            table: "recipe_import_drafts",
            column: "created_recipe_id",
            unique: true,
            filter: "created_recipe_id IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "ix_recipe_import_drafts_owner_user_id_updated_at",
            schema: "imports",
            table: "recipe_import_drafts",
            columns: ["owner_user_id", "updated_at"]);

        migrationBuilder.CreateIndex(
            name: "ix_recipe_import_drafts_target_recipe_id",
            schema: "imports",
            table: "recipe_import_drafts",
            column: "target_recipe_id",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "recipe_import_drafts",
            schema: "imports");
    }
}
