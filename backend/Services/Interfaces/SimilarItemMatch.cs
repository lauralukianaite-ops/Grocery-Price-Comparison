using backend.Entities;

namespace backend.Services.Interfaces;

public record SimilarItemMatch(Item Item, double Score);
