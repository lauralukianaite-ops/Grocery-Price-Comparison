using backend.Enums;

namespace backend.DTOs;

public record DiscountFlagResponseDto(
    int StoreId,
    string StoreName,
    decimal Cost,
    decimal RetailCost,
    decimal? TypicalCost,
    DiscountVerdict Verdict
);