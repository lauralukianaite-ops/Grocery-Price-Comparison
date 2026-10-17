using System.Text.Json;

public class StoreConfigLoader : IStoreConfigLoader
{
    private readonly IFileReader _configFilePathfileReader;

    public StoreConfigLoader(IFileReader fileReader)
    {
        _configFilePathfileReader = fileReader;
    }

    public async Task<Dictionary<string, List<string>>> LoadStoresAsync(string filePath)
    {
        string jsonContent = await _configFilePathfileReader.ReadTextAsync(filePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        return JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonContent, options)
               ?? throw new InvalidOperationException("JSON content is empty or null.");
    }
}