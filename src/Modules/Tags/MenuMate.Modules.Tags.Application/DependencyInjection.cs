using MenuMate.Common.Application;
using MenuMate.Contracts.Tags;
using MenuMate.Modules.Tags.Application.ConfirmTag;
using MenuMate.Modules.Tags.Application.CreateTag;
using MenuMate.Modules.Tags.Application.GetTags;
using MenuMate.Modules.Tags.Application.HideTag;
using Microsoft.Extensions.DependencyInjection;

namespace MenuMate.Modules.Tags.Application;

/// <summary>
/// Регистрация зависимостей прикладного слоя тегов.
/// </summary>
public static class TagsApplicationDependencyInjection
{
    /// <summary>
    /// Добавляет обработчики сценариев Tags.
    /// </summary>
    public static IServiceCollection AddTagsApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<CreateTagCommand, TagResponse>, CreateTagCommandHandler>();
        services.AddScoped<ICommandHandler<ConfirmTagCommand>, ConfirmTagCommandHandler>();
        services.AddScoped<ICommandHandler<HideTagCommand>, HideTagCommandHandler>();
        services.AddScoped<IQueryHandler<GetTagsQuery, IReadOnlyCollection<TagResponse>>, GetTagsQueryHandler>();

        return services;
    }
}
