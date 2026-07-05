using MenuMate.Common.Application;
using MenuMate.Modules.Auth.Infrastructure;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MenuMate.DataImporter.UnitTests;

public sealed class ImporterUserContextTests
{
    [Fact]
    public void ImporterContextOverridesHttpUserContext()
    {
        using var configuration = new ConfigurationManager
        {
            ["ConnectionStrings:Database"] = "Host=localhost;Database=menumate;Username=postgres"
        };
        var importerUserContext = new ImporterUserContext();
        var services = new ServiceCollection();
        services.AddAuthInfrastructure(configuration);
        services.AddSingleton<IUserContext>(importerUserContext);
        using ServiceProvider provider = services.BuildServiceProvider();

        importerUserContext.SetUser(UserId.From(new Guid("01977a00-0000-7000-8000-000000000001")));

        Assert.Same(importerUserContext, provider.GetRequiredService<IUserContext>());
        Assert.Equal(importerUserContext.UserId, provider.GetRequiredService<IUserContext>().UserId);
    }
}
