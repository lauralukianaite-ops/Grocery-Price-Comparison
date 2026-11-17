using backend.Entities;

namespace backend.ValueObjects;

public record CatalogPageResult(List<Item> Items, int TotalCount);