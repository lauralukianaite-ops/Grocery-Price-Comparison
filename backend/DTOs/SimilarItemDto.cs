namespace backend.DTOs;

public record SimilarItemDto(
    int Id,
    string Name,
    string Store,
    double Cost,
    decimal? RetailCost,
    double SimilarityScore);
