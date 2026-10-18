public interface IStoreConfigLoader
{
    Task<Dictionary<string, List<string>>> LoadStoresAsync(string filePath);
}