using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuMate.Modules.Recipes.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "recipes");

        migrationBuilder.CreateTable(
            name: "recipes",
            schema: "recipes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                owner_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                servings = table.Column<int>(type: "integer", nullable: false),
                category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                visibility = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                current_revision_id = table.Column<Guid>(type: "uuid", nullable: false),
                revision_number = table.Column<int>(type: "integer", nullable: false),
                source_recipe_id = table.Column<Guid>(type: "uuid", nullable: true),
                source_revision_id = table.Column<Guid>(type: "uuid", nullable: true),
                total_time_minutes = table.Column<int>(type: "integer", nullable: true),
                active_time_minutes = table.Column<int>(type: "integer", nullable: true),
                source_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_recipes", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "preparation_steps",
            schema: "recipes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                recipe_id = table.Column<Guid>(type: "uuid", nullable: false),
                number = table.Column<int>(type: "integer", nullable: false),
                text = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_preparation_steps", x => x.id);
                table.ForeignKey(
                    name: "fk_preparation_steps_recipes_recipe_id",
                    column: x => x.recipe_id,
                    principalSchema: "recipes",
                    principalTable: "recipes",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "recipe_images",
            schema: "recipes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                owner_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                recipe_id = table.Column<Guid>(type: "uuid", nullable: false),
                scope = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                step_number = table.Column<int>(type: "integer", nullable: true),
                bucket_name = table.Column<string>(type: "character varying(63)", maxLength: 63, nullable: false),
                object_key = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                size_bytes = table.Column<long>(type: "bigint", nullable: false),
                original_file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                alt_text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_recipe_images", x => x.id);
                table.CheckConstraint("ck_recipe_images_step_number_matches_scope", "(scope = 'Step' AND step_number IS NOT NULL)\nOR (scope = 'Cover' AND step_number IS NULL)");
                table.ForeignKey(
                    name: "fk_recipe_images_recipes_recipe_id",
                    column: x => x.recipe_id,
                    principalSchema: "recipes",
                    principalTable: "recipes",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "recipe_ingredients",
            schema: "recipes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                recipe_id = table.Column<Guid>(type: "uuid", nullable: false),
                order = table.Column<int>(type: "integer", nullable: false),
                ingredient_id = table.Column<Guid>(type: "uuid", nullable: false),
                product_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                normalized_product_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                amount = table.Column<decimal>(type: "numeric", nullable: true),
                unit = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                is_optional = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_recipe_ingredients", x => x.id);
                table.ForeignKey(
                    name: "fk_recipe_ingredients_recipes_recipe_id",
                    column: x => x.recipe_id,
                    principalSchema: "recipes",
                    principalTable: "recipes",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "recipe_library_entries",
            schema: "recipes",
            columns: table => new
            {
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                recipe_id = table.Column<Guid>(type: "uuid", nullable: false),
                is_favorite = table.Column<bool>(type: "boolean", nullable: false),
                saved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_recipe_library_entries", x => new { x.user_id, x.recipe_id });
                table.ForeignKey(
                    name: "fk_recipe_library_entries_recipes_recipe_id",
                    column: x => x.recipe_id,
                    principalSchema: "recipes",
                    principalTable: "recipes",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "recipe_revisions",
            schema: "recipes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                recipe_id = table.Column<Guid>(type: "uuid", nullable: false),
                revision_number = table.Column<int>(type: "integer", nullable: false),
                title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                servings = table.Column<int>(type: "integer", nullable: false),
                category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                total_time_minutes = table.Column<int>(type: "integer", nullable: true),
                active_time_minutes = table.Column<int>(type: "integer", nullable: true),
                source_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_recipe_revisions", x => x.id);
                table.ForeignKey(
                    name: "fk_recipe_revisions_recipes_recipe_id",
                    column: x => x.recipe_id,
                    principalSchema: "recipes",
                    principalTable: "recipes",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "recipe_tags",
            schema: "recipes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                recipe_id = table.Column<Guid>(type: "uuid", nullable: false),
                value = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                normalized_value = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_recipe_tags", x => x.id);
                table.ForeignKey(
                    name: "fk_recipe_tags_recipes_recipe_id",
                    column: x => x.recipe_id,
                    principalSchema: "recipes",
                    principalTable: "recipes",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "recipe_revision_ingredients",
            schema: "recipes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                recipe_revision_id = table.Column<Guid>(type: "uuid", nullable: false),
                order = table.Column<int>(type: "integer", nullable: false),
                ingredient_id = table.Column<Guid>(type: "uuid", nullable: false),
                product_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                normalized_product_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                amount = table.Column<decimal>(type: "numeric", nullable: true),
                unit = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                is_optional = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_recipe_revision_ingredients", x => x.id);
                table.ForeignKey(
                    name: "fk_recipe_revision_ingredients_recipe_revisions_recipe_revisio",
                    column: x => x.recipe_revision_id,
                    principalSchema: "recipes",
                    principalTable: "recipe_revisions",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "recipe_revision_steps",
            schema: "recipes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                recipe_revision_id = table.Column<Guid>(type: "uuid", nullable: false),
                number = table.Column<int>(type: "integer", nullable: false),
                text = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_recipe_revision_steps", x => x.id);
                table.ForeignKey(
                    name: "fk_recipe_revision_steps_recipe_revisions_recipe_revision_id",
                    column: x => x.recipe_revision_id,
                    principalSchema: "recipes",
                    principalTable: "recipe_revisions",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "recipe_revision_tags",
            schema: "recipes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                recipe_revision_id = table.Column<Guid>(type: "uuid", nullable: false),
                value = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                normalized_value = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_recipe_revision_tags", x => x.id);
                table.ForeignKey(
                    name: "fk_recipe_revision_tags_recipe_revisions_recipe_revision_id",
                    column: x => x.recipe_revision_id,
                    principalSchema: "recipes",
                    principalTable: "recipe_revisions",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_preparation_steps_recipe_id_number",
            schema: "recipes",
            table: "preparation_steps",
            columns: ["recipe_id", "number"],
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_recipe_images_object_key",
            schema: "recipes",
            table: "recipe_images",
            column: "object_key",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_recipe_images_owner_user_id",
            schema: "recipes",
            table: "recipe_images",
            column: "owner_user_id");

        migrationBuilder.CreateIndex(
            name: "ix_recipe_images_recipe_id",
            schema: "recipes",
            table: "recipe_images",
            column: "recipe_id");

        migrationBuilder.CreateIndex(
            name: "ix_recipe_images_recipe_id_scope",
            schema: "recipes",
            table: "recipe_images",
            columns: ["recipe_id", "scope"]);

        migrationBuilder.CreateIndex(
            name: "ix_recipe_ingredients_ingredient_id",
            schema: "recipes",
            table: "recipe_ingredients",
            column: "ingredient_id");

        migrationBuilder.CreateIndex(
            name: "ix_recipe_ingredients_normalized_product_name",
            schema: "recipes",
            table: "recipe_ingredients",
            column: "normalized_product_name");

        migrationBuilder.CreateIndex(
            name: "ix_recipe_ingredients_recipe_id",
            schema: "recipes",
            table: "recipe_ingredients",
            column: "recipe_id");

        migrationBuilder.CreateIndex(
            name: "ix_recipe_library_entries_recipe_id",
            schema: "recipes",
            table: "recipe_library_entries",
            column: "recipe_id");

        migrationBuilder.CreateIndex(
            name: "ix_recipe_library_entries_user_id_is_favorite",
            schema: "recipes",
            table: "recipe_library_entries",
            columns: ["user_id", "is_favorite"]);

        migrationBuilder.CreateIndex(
            name: "ix_recipe_revision_ingredients_recipe_revision_id_order",
            schema: "recipes",
            table: "recipe_revision_ingredients",
            columns: ["recipe_revision_id", "order"],
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_recipe_revision_steps_recipe_revision_id_number",
            schema: "recipes",
            table: "recipe_revision_steps",
            columns: ["recipe_revision_id", "number"],
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_recipe_revision_tags_recipe_revision_id_normalized_value",
            schema: "recipes",
            table: "recipe_revision_tags",
            columns: ["recipe_revision_id", "normalized_value"],
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_recipe_revisions_recipe_id_revision_number",
            schema: "recipes",
            table: "recipe_revisions",
            columns: ["recipe_id", "revision_number"],
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_recipe_tags_normalized_value",
            schema: "recipes",
            table: "recipe_tags",
            column: "normalized_value");

        migrationBuilder.CreateIndex(
            name: "ix_recipe_tags_recipe_id",
            schema: "recipes",
            table: "recipe_tags",
            column: "recipe_id");

        migrationBuilder.CreateIndex(
            name: "ix_recipes_category",
            schema: "recipes",
            table: "recipes",
            column: "category");

        migrationBuilder.CreateIndex(
            name: "ix_recipes_current_revision_id",
            schema: "recipes",
            table: "recipes",
            column: "current_revision_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_recipes_is_deleted",
            schema: "recipes",
            table: "recipes",
            column: "is_deleted");

        migrationBuilder.CreateIndex(
            name: "ix_recipes_owner_user_id",
            schema: "recipes",
            table: "recipes",
            column: "owner_user_id");

        migrationBuilder.CreateIndex(
            name: "ix_recipes_source_recipe_id",
            schema: "recipes",
            table: "recipes",
            column: "source_recipe_id");

        migrationBuilder.CreateIndex(
            name: "ix_recipes_title",
            schema: "recipes",
            table: "recipes",
            column: "title");

        migrationBuilder.CreateIndex(
            name: "ix_recipes_visibility",
            schema: "recipes",
            table: "recipes",
            column: "visibility");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "preparation_steps",
            schema: "recipes");

        migrationBuilder.DropTable(
            name: "recipe_images",
            schema: "recipes");

        migrationBuilder.DropTable(
            name: "recipe_ingredients",
            schema: "recipes");

        migrationBuilder.DropTable(
            name: "recipe_library_entries",
            schema: "recipes");

        migrationBuilder.DropTable(
            name: "recipe_revision_ingredients",
            schema: "recipes");

        migrationBuilder.DropTable(
            name: "recipe_revision_steps",
            schema: "recipes");

        migrationBuilder.DropTable(
            name: "recipe_revision_tags",
            schema: "recipes");

        migrationBuilder.DropTable(
            name: "recipe_tags",
            schema: "recipes");

        migrationBuilder.DropTable(
            name: "recipe_revisions",
            schema: "recipes");

        migrationBuilder.DropTable(
            name: "recipes",
            schema: "recipes");
    }
}
