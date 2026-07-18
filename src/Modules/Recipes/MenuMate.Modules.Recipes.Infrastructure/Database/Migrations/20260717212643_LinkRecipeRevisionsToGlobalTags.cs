using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuMate.Modules.Recipes.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class LinkRecipeRevisionsToGlobalTags : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            WITH tag_occurrences AS (
                SELECT
                    revision.recipe_id,
                    revision_tag.value,
                    revision_tag.normalized_value
                FROM recipes.recipe_revision_tags AS revision_tag
                INNER JOIN recipes.recipe_revisions AS revision
                    ON revision.id = revision_tag.recipe_revision_id

                UNION ALL

                SELECT
                    recipe_tag.recipe_id,
                    recipe_tag.value,
                    recipe_tag.normalized_value
                FROM recipes.recipe_tags AS recipe_tag
            ),
            ranked_names AS (
                SELECT
                    occurrence.normalized_value,
                    occurrence.value,
                    COUNT(*) AS use_count,
                    ROW_NUMBER() OVER (
                        PARTITION BY occurrence.normalized_value
                        ORDER BY
                            COUNT(*) DESC,
                            LENGTH(occurrence.value),
                            LOWER(occurrence.value),
                            occurrence.value
                    ) AS name_rank
                FROM tag_occurrences AS occurrence
                GROUP BY
                    occurrence.normalized_value,
                    occurrence.value
            ),
            source_kinds AS (
                SELECT
                    occurrence.normalized_value,
                    BOOL_AND(import_draft.id IS NOT NULL) AS generated_only
                FROM tag_occurrences AS occurrence
                LEFT JOIN imports.recipe_import_drafts AS import_draft
                    ON import_draft.created_recipe_id = occurrence.recipe_id
                GROUP BY occurrence.normalized_value
            ),
            catalog_entries AS (
                SELECT
                    ranked.normalized_value,
                    ranked.value,
                    source_kind.generated_only
                FROM ranked_names AS ranked
                INNER JOIN source_kinds AS source_kind
                    ON source_kind.normalized_value = ranked.normalized_value
                WHERE ranked.name_rank = 1
            )
            INSERT INTO tags.tags (
                id,
                name,
                normalized_name,
                kind,
                status,
                created_at,
                updated_at)
            SELECT
                MD5('menumate:tag:' || entry.normalized_value)::uuid,
                entry.value,
                entry.normalized_value,
                CASE WHEN entry.generated_only THEN 'Suggested' ELSE 'User' END,
                'Confirmed',
                NOW(),
                NOW()
            FROM catalog_entries AS entry
            ON CONFLICT (normalized_name) DO UPDATE
            SET
                status = 'Confirmed',
                updated_at = EXCLUDED.updated_at
            WHERE tags.tags.status = 'Proposed';

            CREATE TABLE recipes.recipe_revision_tags_v2 (
                recipe_revision_id uuid NOT NULL,
                tag_id uuid NOT NULL
            );

            INSERT INTO recipes.recipe_revision_tags_v2 (recipe_revision_id, tag_id)
            SELECT DISTINCT
                source.recipe_revision_id,
                catalog_tag.id
            FROM (
                SELECT
                    revision_tag.recipe_revision_id,
                    revision_tag.normalized_value
                FROM recipes.recipe_revision_tags AS revision_tag

                UNION ALL

                SELECT
                    recipe.current_revision_id,
                    recipe_tag.normalized_value
                FROM recipes.recipe_tags AS recipe_tag
                INNER JOIN recipes.recipes AS recipe
                    ON recipe.id = recipe_tag.recipe_id
            ) AS source
            INNER JOIN tags.tags AS catalog_tag
                ON catalog_tag.normalized_name = source.normalized_value;

            DROP TABLE recipes.recipe_revision_tags;
            ALTER TABLE recipes.recipe_revision_tags_v2 RENAME TO recipe_revision_tags;

            ALTER TABLE recipes.recipe_revision_tags
                ADD CONSTRAINT pk_recipe_revision_tags
                PRIMARY KEY (recipe_revision_id, tag_id);

            ALTER TABLE recipes.recipe_revision_tags
                ADD CONSTRAINT fk_recipe_revision_tags_recipe_revisions_recipe_revision_id
                FOREIGN KEY (recipe_revision_id)
                REFERENCES recipes.recipe_revisions (id)
                ON DELETE CASCADE;

            ALTER TABLE recipes.recipe_revision_tags
                ADD CONSTRAINT fk_recipe_revision_tags_tags_tag_id
                FOREIGN KEY (tag_id)
                REFERENCES tags.tags (id)
                ON DELETE RESTRICT;

            CREATE INDEX ix_recipe_revision_tags_tag_id_recipe_revision_id
                ON recipes.recipe_revision_tags (tag_id, recipe_revision_id);

            DROP TABLE recipes.recipe_tags;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            CREATE TABLE recipes.recipe_revision_tags_legacy (
                id uuid NOT NULL,
                recipe_revision_id uuid NOT NULL,
                value character varying(64) NOT NULL,
                normalized_value character varying(64) NOT NULL
            );

            INSERT INTO recipes.recipe_revision_tags_legacy (
                id,
                recipe_revision_id,
                value,
                normalized_value)
            SELECT
                MD5(
                    'menumate:recipe-revision-tag:' ||
                    revision_tag.recipe_revision_id::text || ':' ||
                    revision_tag.tag_id::text)::uuid,
                revision_tag.recipe_revision_id,
                catalog_tag.name,
                catalog_tag.normalized_name
            FROM recipes.recipe_revision_tags AS revision_tag
            INNER JOIN tags.tags AS catalog_tag
                ON catalog_tag.id = revision_tag.tag_id;

            CREATE TABLE recipes.recipe_tags (
                id uuid NOT NULL,
                recipe_id uuid NOT NULL,
                value character varying(64) NOT NULL,
                normalized_value character varying(64) NOT NULL,
                CONSTRAINT pk_recipe_tags PRIMARY KEY (id),
                CONSTRAINT fk_recipe_tags_recipes_recipe_id
                    FOREIGN KEY (recipe_id)
                    REFERENCES recipes.recipes (id)
                    ON DELETE CASCADE
            );

            INSERT INTO recipes.recipe_tags (id, recipe_id, value, normalized_value)
            SELECT
                MD5(
                    'menumate:recipe-tag:' ||
                    recipe.id::text || ':' ||
                    revision_tag.tag_id::text)::uuid,
                recipe.id,
                catalog_tag.name,
                catalog_tag.normalized_name
            FROM recipes.recipes AS recipe
            INNER JOIN recipes.recipe_revision_tags AS revision_tag
                ON revision_tag.recipe_revision_id = recipe.current_revision_id
            INNER JOIN tags.tags AS catalog_tag
                ON catalog_tag.id = revision_tag.tag_id;

            DROP TABLE recipes.recipe_revision_tags;
            ALTER TABLE recipes.recipe_revision_tags_legacy RENAME TO recipe_revision_tags;

            ALTER TABLE recipes.recipe_revision_tags
                ADD CONSTRAINT pk_recipe_revision_tags PRIMARY KEY (id);

            ALTER TABLE recipes.recipe_revision_tags
                ADD CONSTRAINT fk_recipe_revision_tags_recipe_revisions_recipe_revision_id
                FOREIGN KEY (recipe_revision_id)
                REFERENCES recipes.recipe_revisions (id)
                ON DELETE CASCADE;

            CREATE UNIQUE INDEX ix_recipe_revision_tags_recipe_revision_id_normalized_value
                ON recipes.recipe_revision_tags (recipe_revision_id, normalized_value);

            CREATE INDEX ix_recipe_tags_normalized_value
                ON recipes.recipe_tags (normalized_value);

            CREATE INDEX ix_recipe_tags_recipe_id
                ON recipes.recipe_tags (recipe_id);
            """);
    }
}
