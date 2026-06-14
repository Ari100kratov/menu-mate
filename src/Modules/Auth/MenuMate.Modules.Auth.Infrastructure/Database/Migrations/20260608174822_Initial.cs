using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MenuMate.Modules.Auth.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "auth");

        migrationBuilder.CreateTable(
            name: "roles",
            schema: "auth",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_roles", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "users",
            schema: "auth",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                display_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                password_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_users", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "refresh_tokens",
            schema: "auth",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                value = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                is_revoked = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_refresh_tokens", x => x.id);
                table.ForeignKey(
                    name: "fk_refresh_tokens_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "auth",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "user_roles",
            schema: "auth",
            columns: table => new
            {
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                role_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_roles", x => new { x.user_id, x.role_id });
                table.ForeignKey(
                    name: "fk_user_roles_roles_role_id",
                    column: x => x.role_id,
                    principalSchema: "auth",
                    principalTable: "roles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_user_roles_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "auth",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.InsertData(
            schema: "auth",
            table: "roles",
            columns: ["id", "name"],
            values: new object[,]
            {
                { new Guid("018f0000-0000-7000-8000-000000000001"), "user" },
                { new Guid("018f0000-0000-7000-8000-000000000002"), "admin" }
            });

        migrationBuilder.CreateIndex(
            name: "ix_refresh_tokens_user_id",
            schema: "auth",
            table: "refresh_tokens",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "ix_refresh_tokens_value",
            schema: "auth",
            table: "refresh_tokens",
            column: "value",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_roles_name",
            schema: "auth",
            table: "roles",
            column: "name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_user_roles_role_id",
            schema: "auth",
            table: "user_roles",
            column: "role_id");

        migrationBuilder.CreateIndex(
            name: "ix_users_email",
            schema: "auth",
            table: "users",
            column: "email",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "refresh_tokens",
            schema: "auth");

        migrationBuilder.DropTable(
            name: "user_roles",
            schema: "auth");

        migrationBuilder.DropTable(
            name: "roles",
            schema: "auth");

        migrationBuilder.DropTable(
            name: "users",
            schema: "auth");
    }
}
