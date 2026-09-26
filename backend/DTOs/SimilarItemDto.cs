namespace backend.DTOs;

public record SimilarItemDto(
    int Id,
    string Name,
    string Store,
    double Price,
    double SimilarityScore
);
