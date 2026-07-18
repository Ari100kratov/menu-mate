using MenuMate.Modules.Tags.Domain.Enums;
using MenuMate.Modules.Tags.Domain.Models;
using MenuMate.Modules.Tags.Domain.ValueObjects;

namespace MenuMate.Modules.Tags.Domain.UnitTests.Models;

public sealed class TagTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 6, 12, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(TagKind.System, TagStatus.Confirmed)]
    [InlineData(TagKind.User, TagStatus.Confirmed)]
    [InlineData(TagKind.Suggested, TagStatus.Confirmed)]
    public void CreateShouldSetStatusFromKind(TagKind kind, TagStatus expectedStatus)
    {
        var tag = Tag.Create(Guid.CreateVersion7(), TagName.Create("Быстро").Value, kind, CreatedAt);

        Assert.Equal(expectedStatus, tag.Status);
        Assert.Equal(CreatedAt, tag.UpdatedAt);
    }

    [Fact]
    public void ConfirmAndHideShouldUpdateStatusAndTimestamp()
    {
        var tag = Tag.Create(
            Guid.CreateVersion7(),
            TagName.Create("Сезонное").Value,
            TagKind.Suggested,
            CreatedAt);

        tag.Confirm(CreatedAt.AddMinutes(1));
        Assert.Equal(TagStatus.Confirmed, tag.Status);
        Assert.Equal(CreatedAt.AddMinutes(1), tag.UpdatedAt);

        tag.Hide(CreatedAt.AddMinutes(2));
        Assert.Equal(TagStatus.Hidden, tag.Status);
        Assert.Equal(CreatedAt.AddMinutes(2), tag.UpdatedAt);
    }
}
