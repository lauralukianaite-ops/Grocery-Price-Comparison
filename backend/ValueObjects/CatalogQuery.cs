using backend.Enums;

namespace backend.ValueObjects;

public record CatalogQuery(
    int Page,
    int PageSize,
    string? Search,
    string? Store,
    CatalogSort Sort,
    List<int>? ItemIds);