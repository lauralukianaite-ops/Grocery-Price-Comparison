using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using backend.Entities;
using backend.Services.Interfaces;

namespace backend.Tests.Integration;

// Stands in for Google in tests: answers with the road km written here, found by shop address, and counts the calls.
public class FixedDistanceProvider : IDistanceProvider
{
    public List<(string Address, double? Km)> RoadKm { get; } = new();
    public bool Fails { get; set; }
    public int Calls { get; private set; }

    public Task<List<double?>> RoadDistanceKmAsync(double userLatitude, double userLongitude, List<StoreLocation> destinations)
    {
        Calls++;
        if (Fails) throw new HttpRequestException("Google does not answer.");

        var km = destinations
            .Select(d => RoadKm.First(r => r.Address == d.Address).Km)
            .ToList();
        return Task.FromResult(km);
    }
}
