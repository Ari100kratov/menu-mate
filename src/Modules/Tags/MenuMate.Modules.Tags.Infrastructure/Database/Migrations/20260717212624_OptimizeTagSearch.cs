using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuMate.Modules.Tags.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class OptimizeTagSearch : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

        migrationBuilder.Sql(
            """
            CREATE INDEX ix_tags_normalized_name_trgm
                ON tags.tags
                USING gin (normalized_name gin_trgm_ops);
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_tags_normalized_name_trgm",
            schema: "tags",
            table: "tags");

        migrationBuilder.AlterDatabase()
            .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,");
    }
}
