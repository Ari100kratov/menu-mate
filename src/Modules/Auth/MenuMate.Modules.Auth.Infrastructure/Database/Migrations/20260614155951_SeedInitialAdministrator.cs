using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuMate.Modules.Auth.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class SeedInitialAdministrator : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            INSERT INTO auth.users (
                id,
                email,
                display_name,
                password_hash,
                created_at,
                updated_at)
            VALUES (
                '01977a00-0000-7000-8000-000000000001',
                'ari100kratov@yandex.ru',
                'Администратор',
                '9062E2EF52C6E277DC7016CF4F0E700BF270854B952CB698F34BB6CD502E0A32-6D656E756D6174652D61646D696E3031',
                TIMESTAMPTZ '2026-06-14 00:00:00+00',
                TIMESTAMPTZ '2026-06-14 00:00:00+00')
            ON CONFLICT (email) DO NOTHING;

            INSERT INTO auth.user_roles (user_id, role_id)
            SELECT users.id, roles.id
            FROM auth.users AS users
            CROSS JOIN auth.roles AS roles
            WHERE users.email = 'ari100kratov@yandex.ru'
            ON CONFLICT (user_id, role_id) DO NOTHING;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DELETE FROM auth.users
            WHERE id = '01977a00-0000-7000-8000-000000000001'
              AND email = 'ari100kratov@yandex.ru';
            """);
    }
}
