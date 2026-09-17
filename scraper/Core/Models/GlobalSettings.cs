namespace Scraper.Core.Settings;

public sealed class GlobalSettings
{
    public const string SectionName = "GlobalSettings";

    public bool TestingMode { get; set; } = false;
    public string DevelopmentDatabaseName { get; set; } = string.Empty;
    public string ProductionDatabaseName { get; set; } = string.Empty;
    public string DatabaseUser { get; set; } = string.Empty;
    public string DatabasePassword { get; set; } = string.Empty;
    public string DatabaseHost { get; set; } = string.Empty;
    public int DatabasePort {get; set;} = 0;
}