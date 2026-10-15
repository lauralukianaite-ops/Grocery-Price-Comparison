namespace backend.DTOs;

public record BasketResponseDto(
    Guid Id,
    int StoreId,
    string StoreName,
    decimal Total,
    DateTime CreatedAt,
    BasketStrictness Strictness,
    List<BasketRowDto> Rows,
    List<BasketRequestItemDto> RequestItems
);
