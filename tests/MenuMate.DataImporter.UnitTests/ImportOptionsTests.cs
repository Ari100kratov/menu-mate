using Microsoft.Extensions.Configuration;

namespace MenuMate.DataImporter.UnitTests;

public sealed class ImportOptionsTests
{
    [Fact]
    public void CreateReadsConnectionStringFromCommandLine()
    {
        using var configuration = new ConfigurationManager
        {
            ["DataImport:ApiUrl"] = "https://ru.wikibooks.org/w/api.php",
            ["DataImport:SiteUrl"] = "https://ru.wikibooks.org/wiki/"
        };

        var options = ImportOptions.Create(
            configuration,
            ["--connection-string", "Host=localhost;Database=menumate", "--dry-run"]);

        Assert.Equal("Host=localhost;Database=menumate", options.ConnectionString);
        Assert.True(options.DryRun);
    }
}
