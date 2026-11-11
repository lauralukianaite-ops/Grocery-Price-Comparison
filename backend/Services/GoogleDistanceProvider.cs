using System.Text;
using System.Text.Json;
using backend.Entities;
using backend.Services.Interfaces;

namespace backend.Services;

public class GoogleDistanceProvider : IDistanceProvider
{
    private readonly HttpClient _httpClient;
    
    public GoogleDistanceProvider(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("X-Goog-Api-Key", configuration["GoogleMaps:ApiKey"]);
        _httpClient.DefaultRequestHeaders.Add("X-Goog-FieldMask", "originIndex,destinationIndex,distanceMeters,condition");
    }
    
    public async Task<List<double?>> RoadDistanceKmAsync(double userLatitude, double userLongitude, List<StoreLocation> destinations)
    {
        var url = "https://routes.googleapis.com/distanceMatrix/v2:computeRouteMatrix";

        var request = new
        {
            origins = new List<object>
            {
                new { waypoint = new { location = new { latLng = new { latitude = userLatitude, longitude = userLongitude } } } }
            },
            destinations = destinations
                .Select(d => new { waypoint = new { location = new { latLng = new { latitude = d.Latitude, longitude = d.Longitude } } } })
                .ToList(),
            travelMode = "DRIVE"
        };
        var body = JsonSerializer.Serialize(request);

        var content = new StringContent(body, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var elements = doc.RootElement.EnumerateArray();

        return elements
            .Select(element => (
                Index: element.GetProperty("destinationIndex").GetInt32(),
                Km: element.TryGetProperty("distanceMeters", out var meters)
                    ? meters.GetDouble() / 1000
                    : (double?)null))
            .OrderBy(pair => pair.Index)
            .Select(pair => pair.Km)
            .ToList();
    }
}