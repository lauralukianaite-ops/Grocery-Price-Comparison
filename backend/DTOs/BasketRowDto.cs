namespace backend.DTOs;

public record BasketRowDto(
    int Position,
    int ItemId,
    string ItemName,
    int Quantity,
    decimal UnitCost,
    decimal RowTotal,
    DiscountVerdict Verdict
);
