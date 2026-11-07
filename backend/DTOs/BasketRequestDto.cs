namespace backend.DTOs;

public record BasketRequestDto(
    List<BasketRequestItemDto>? Items, 
    string? Strictness, 
    List<int>? BannedIds = null,
    double? Latitude = null,
    double? Longitude = null,
    decimal? CostPerKm = null,
    double? MaxKm = null
);
