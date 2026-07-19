using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuMate.Modules.MenuPlanning.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class BackfillDefaultMealSlots : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            INSERT INTO menu_planning.meal_slots (
                id,
                owner_user_id,
                name,
                sort_order,
                created_at,
                updated_at)
            SELECT
                md5(users.id::text || ':menumate:default-meal-slot:' || defaults.name)::uuid,
                users.id,
                defaults.name,
                defaults.sort_order,
                CURRENT_TIMESTAMP,
                CURRENT_TIMESTAMP
            FROM auth.users AS users
            CROSS JOIN (VALUES
                ('Завтрак', 0),
                ('Обед', 1),
                ('Ужин', 2)) AS defaults(name, sort_order)
            WHERE NOT EXISTS (
                SELECT 1
                FROM menu_planning.meal_slots AS existing
                WHERE existing.owner_user_id = users.id)
            ON CONFLICT (id) DO NOTHING;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // The backfill is intentionally preserved because users can rename and use these slots.
    }
}
