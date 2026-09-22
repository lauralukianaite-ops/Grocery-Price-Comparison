namespace backend.DTOs;

public record PricePointDto(
    int StoreId,
    string StoreName,
    decimal Amount,
    DateTime RecordedAt
);