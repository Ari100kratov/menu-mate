using MenuMate.Common.Application;

namespace MenuMate.Modules.Tags.Application.HideTag;

internal sealed record HideTagCommand(Guid TagId) : ICommand;
