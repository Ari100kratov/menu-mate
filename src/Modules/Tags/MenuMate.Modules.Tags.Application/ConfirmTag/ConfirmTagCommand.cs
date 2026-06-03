using MenuMate.Common.Application;

namespace MenuMate.Modules.Tags.Application.ConfirmTag;

internal sealed record ConfirmTagCommand(Guid TagId) : ICommand;
