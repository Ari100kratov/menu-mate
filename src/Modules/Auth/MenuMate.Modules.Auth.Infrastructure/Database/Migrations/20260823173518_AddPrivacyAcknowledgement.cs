using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuMate.Modules.Auth.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class AddPrivacyAcknowledgement : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "privacy_policy_accepted_at",
            schema: "auth",
            table: "users",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "privacy_policy_accepted_version",
            schema: "auth",
            table: "users",
            type: "character varying(32)",
            maxLength: 32,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "privacy_policy_accepted_at",
            schema: "auth",
            table: "users");

        migrationBuilder.DropColumn(
            name: "privacy_policy_accepted_version",
            schema: "auth",
            table: "users");
    }
}
