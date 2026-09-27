namespace backend.DTOs;

public enum DiscountVerdict
{
    Real,
    Exaggerated,
    False
}

public record DiscountFlagResponseDto(
    int StoreId,
    string StoreName,
    decimal Cost,
    decimal RetailCost,
    decimal TypicalCost,
    DiscountVerdict Verdict
);