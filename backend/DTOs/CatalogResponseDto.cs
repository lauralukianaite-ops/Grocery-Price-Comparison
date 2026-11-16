namespace backend.DTOs;

public record CatalogOfferDto(
    int StoreId,
    string StoreName,
    decimal Cost,
    decimal? RetailCost,
    DateTime RecordedAt
);

public record CatalogItemDto(
    int Id,
    string Name,
    List<CatalogOfferDto> Offers
);

public record CatalogResponseDto(
    List<CatalogItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);

