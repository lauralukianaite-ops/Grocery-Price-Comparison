namespace backend.DTOs;

public record PricePointDto(
    int StoreId,
    string StoreName,
    decimal Cost,
    decimal? RetailCost,
    DateTime RecordedAt);