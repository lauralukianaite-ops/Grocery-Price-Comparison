namespace backend.Services;

public class DbConfigService
{
    public bool IsTestingMode { get; private set; }

    public DbConfigService()
    {
        string? testingModeValue = Environment.GetEnvironmentVariable("TestingMode");
        IsTestingMode = bool.TryParse(testingModeValue, out bool result) && result;
    }

    public string GetEnvironmentStatus()
    {
        if (IsTestingMode)
        {
            return "TestingMode is active: connecting to the Development database.";
        }
        
        return "Production mode is active: connecting to the main database.";
    }
}