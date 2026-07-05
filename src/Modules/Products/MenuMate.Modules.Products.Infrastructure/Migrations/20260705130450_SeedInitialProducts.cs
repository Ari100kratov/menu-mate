using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using MenuMate.Modules.Products.Infrastructure.SeedData;
using MenuMate.SharedKernel;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuMate.Modules.Products.Infrastructure.Migrations;

/// <inheritdoc />
public partial class SeedInitialProducts : Migration
{
    private static readonly DateTimeOffset SeededAt = new(2026, 7, 5, 0, 0, 0, TimeSpan.Zero);

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        foreach (ProductSeedEntry product in ProductSeedCatalog.Products)
        {
            migrationBuilder.Sql(
                $"""
                INSERT INTO products.products (
                    id,
                    name,
                    normalized_name,
                    category,
                    created_at)
                VALUES (
                    {SqlString(CreateProductId(product).ToString())},
                    {SqlString(product.Name)},
                    {SqlString(TextNormalizer.NormalizeSearchText(product.Name))},
                    {SqlString(product.Category)},
                    {SqlTimestamp(SeededAt)})
                ON CONFLICT (normalized_name, category) DO NOTHING;
                """);
        }
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        foreach (ProductSeedEntry product in ProductSeedCatalog.Products)
        {
            migrationBuilder.Sql(
                $"""
                DELETE FROM products.products
                WHERE id = {SqlString(CreateProductId(product).ToString())};
                """);
        }
    }

    private static Guid CreateProductId(ProductSeedEntry product)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{product.Category}\n{product.Name}"));
        Span<byte> bytes = stackalloc byte[16];
        hash.AsSpan(0, 16).CopyTo(bytes);
        bytes[7] = (byte)((bytes[7] & 0x0F) | 0x50);
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);
        return new Guid(bytes);
    }

    private static string SqlString(string value) =>
        $"'{value.Replace("'", "''", StringComparison.Ordinal)}'";

    private static string SqlTimestamp(DateTimeOffset value)
    {
        string formatted = value
            .ToUniversalTime()
            .ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        return $"TIMESTAMPTZ {SqlString($"{formatted}+00")}";
    }
}
