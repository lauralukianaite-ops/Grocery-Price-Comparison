public class DiskFileReader : IFileReader
{
    public async Task<string> ReadTextAsync(string filePath)
    { 
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }
        return await File.ReadAllTextAsync(filePath);
    }
}