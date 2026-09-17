namespace Scraper.Core.Services;

using Microsoft.Extensions.Options;
using Scraper.Core.Interfaces;
using Scraper.Core.Settings;
public class DatabaseConnectionFactory : IDatabaseConnectionFactory 
{
    private readonly GlobalSettings _globalSettings;

    public DatabaseConnectionFactory(IOptions<GlobalSettings> settings)
    {
        _globalSettings = settings.Value;
    }

    public void TestOptions()
    {
        Console.WriteLine($"TestingMode: {_globalSettings.TestingMode}");
        Console.WriteLine($"Dev DB: {_globalSettings.DevelopmentDatabaseName}");
        Console.WriteLine($"User: {_globalSettings.DatabaseUser}");
        Console.WriteLine($"Pass: {_globalSettings.DatabasePassword}");
        Console.WriteLine($"Port: {_globalSettings.DatabasePort}");
     }
}