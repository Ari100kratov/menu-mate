using MenuMate.Modules.Tags.Domain.Enums;
using MenuMate.Modules.Tags.Domain.Models;
using MenuMate.Modules.Tags.Domain.ValueObjects;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Tags.Infrastructure.Database.Entities;

internal sealed class TagRecord
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string NormalizedName { get; set; } = string.Empty;

    public TagKind Kind { get; set; }

    public TagStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public static TagRecord FromDomain(Tag tag) =>
        new()
        {
            Id = tag.Id,
            Name = tag.Name.Value,
            NormalizedName = tag.Name.NormalizedValue,
            Kind = tag.Kind,
            Status = tag.Status,
            CreatedAt = tag.CreatedAt,
            UpdatedAt = tag.UpdatedAt
        };

    public void Apply(Tag tag)
    {
        Name = tag.Name.Value;
        NormalizedName = tag.Name.NormalizedValue;
        Kind = tag.Kind;
        Status = tag.Status;
        UpdatedAt = tag.UpdatedAt;
    }

    public Tag ToDomain()
    {
        Result<TagName> name = TagName.Create(Name);
        if (name.IsFailure)
        {
            throw new DomainException(name.Error);
        }

        return Tag.Rehydrate(
            Id,
            name.Value,
            Kind,
            Status,
            CreatedAt,
            UpdatedAt);
    }
}
