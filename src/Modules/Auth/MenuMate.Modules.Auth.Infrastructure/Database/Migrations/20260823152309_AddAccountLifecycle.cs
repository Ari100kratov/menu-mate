using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuMate.Modules.Auth.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class AddAccountLifecycle : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "email_verification_status",
            schema: "auth",
            table: "users",
            type: "character varying(32)",
            maxLength: 32,
            nullable: false,
            defaultValue: "LegacyUnverified");

        migrationBuilder.Sql(
            "ALTER TABLE auth.users ALTER COLUMN email_verification_status DROP DEFAULT;");

        migrationBuilder.CreateTable(
            name: "account_actions",
            schema: "auth",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                purpose = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                target_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                secret_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                resend_available_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                failed_attempts = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_account_actions", x => x.id);
                table.ForeignKey(
                    name: "fk_account_actions_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "auth",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_account_actions_purpose_secret_hash",
            schema: "auth",
            table: "account_actions",
            columns: ["purpose", "secret_hash"],
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_account_actions_user_id_purpose_created_at",
            schema: "auth",
            table: "account_actions",
            columns: ["user_id", "purpose", "created_at"]);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "account_actions",
            schema: "auth");

        migrationBuilder.DropColumn(
            name: "email_verification_status",
            schema: "auth",
            table: "users");
    }
}
