using MenuMate.Common.Application;
using MenuMate.Contracts.Tags;

namespace MenuMate.Modules.Tags.Application.CreateTag;

internal sealed record CreateTagCommand(CreateTagRequest Request) : ICommand<TagResponse>;
