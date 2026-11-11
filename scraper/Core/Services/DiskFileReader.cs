public class DiskFileReader : IFileReader
{
    public Task<string> ReadTextAsync(string filePath)
    {
        return File.ReadAllTextAsync(filePath);
    }
}