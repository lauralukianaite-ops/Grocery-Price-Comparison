namespace backend.DTOs;

public record SimilarItemDto(
    int Id,
    string Name,
    string Category,
    double SimilarityScore
);
