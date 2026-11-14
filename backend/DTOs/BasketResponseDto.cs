using backend.Enums;

namespace backend.DTOs;

public record BasketResponseDto(
    Guid Id,
    int StoreId,
    string StoreName,
    decimal GroceriesTotal,
    decimal TravelCost,
    double? DistanceKm,
    string? LocationAddress,
    decimal GrandTotal,
    DateTime CreatedAt,
    BasketStrictness Strictness,
    List<BasketRowDto> Rows,
    List<BasketRequestItemDto> RequestItems,
    List<int> BannedIds
);
