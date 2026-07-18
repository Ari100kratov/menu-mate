using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuMate.Modules.Recipes.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class SaveRecipeRevisionsInLibrary : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DELETE FROM recipes.recipe_library_entries AS library_entry
            USING recipes.recipes AS recipe
            WHERE recipe.id = library_entry.recipe_id
              AND recipe.owner_user_id = library_entry.user_id
              AND NOT library_entry.is_favorite;
            """);

        migrationBuilder.DropIndex(
            name: "ix_recipe_library_entries_user_id_is_favorite",
            schema: "recipes",
            table: "recipe_library_entries");

        migrationBuilder.AddColumn<Guid>(
            name: "saved_revision_id",
            schema: "recipes",
            table: "recipe_library_entries",
            type: "uuid",
            nullable: true);

        migrationBuilder.Sql(
            """
            UPDATE recipes.recipe_library_entries AS library_entry
            SET saved_revision_id = recipe.current_revision_id
            FROM recipes.recipes AS recipe
            WHERE recipe.id = library_entry.recipe_id;
            """);

        migrationBuilder.AlterColumn<Guid>(
            name: "saved_revision_id",
            schema: "recipes",
            table: "recipe_library_entries",
            type: "uuid",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "uuid",
            oldNullable: true);

        migrationBuilder.DropColumn(
            name: "is_favorite",
            schema: "recipes",
            table: "recipe_library_entries");

        migrationBuilder.CreateIndex(
            name: "ix_recipe_library_entries_saved_revision_id",
            schema: "recipes",
            table: "recipe_library_entries",
            column: "saved_revision_id");

        migrationBuilder.CreateIndex(
            name: "ix_recipe_library_entries_user_id",
            schema: "recipes",
            table: "recipe_library_entries",
            column: "user_id");

        migrationBuilder.AddForeignKey(
            name: "fk_recipe_library_entries_recipe_revisions_saved_revision_id",
            schema: "recipes",
            table: "recipe_library_entries",
            column: "saved_revision_id",
            principalSchema: "recipes",
            principalTable: "recipe_revisions",
            principalColumn: "id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_recipe_library_entries_recipe_revisions_saved_revision_id",
            schema: "recipes",
            table: "recipe_library_entries");

        migrationBuilder.DropIndex(
            name: "ix_recipe_library_entries_saved_revision_id",
            schema: "recipes",
            table: "recipe_library_entries");

        migrationBuilder.DropIndex(
            name: "ix_recipe_library_entries_user_id",
            schema: "recipes",
            table: "recipe_library_entries");

        migrationBuilder.DropColumn(
            name: "saved_revision_id",
            schema: "recipes",
            table: "recipe_library_entries");

        migrationBuilder.AddColumn<bool>(
            name: "is_favorite",
            schema: "recipes",
            table: "recipe_library_entries",
            type: "boolean",
            nullable: false,
            defaultValue: true);

        migrationBuilder.CreateIndex(
            name: "ix_recipe_library_entries_user_id_is_favorite",
            schema: "recipes",
            table: "recipe_library_entries",
            columns: ["user_id", "is_favorite"]);
    }
}
