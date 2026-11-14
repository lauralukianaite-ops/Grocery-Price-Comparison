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
        _httpClient.DefaultRequestHeaders.Add("X-Goog-Api-Key", configuration["GoogleMaps:ApiKey"]); //header for the request, for them to know whos calling and who to bill
        _httpClient.DefaultRequestHeaders.Add("X-Goog-FieldMask", "originIndex,destinationIndex,distanceMeters,condition"); //fields asked for to be in the response
    }
    
    // takes user's location, gives the amount of km by road to each of the locations (takes one near location per store chain)
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
        
        var response = await _httpClient.PostAsJsonAsync(url, request); // google's answer
        response.EnsureSuccessStatusCode(); // throw if error
        var json = await response.Content.ReadAsStringAsync(); // answer as text
        using var doc = JsonDocument.Parse(json); // text to json
        var elements = doc.RootElement.EnumerateArray(); // one per destination

        return elements // give list of km, in same order.
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