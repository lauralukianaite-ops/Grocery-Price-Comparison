using backend.Entities;

namespace backend.Services.Interfaces;

public interface IDistanceProvider
{
    Task<List<double?>> RoadDistanceKmAsync(double userLatitude, double userLongitude, List<StoreLocation> destinations);
}