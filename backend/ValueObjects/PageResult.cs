namespace backend.ValueObjects;

public record PageResult<T>(List<T> Items, int TotalCount);
