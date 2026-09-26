namespace backend.DTOs;

public record PricePointDto(
    int StoreId,
    string StoreName,
    decimal Amount,
    decimal? RetailCost,
    DateTime RecordedAt);