namespace backend.DTOs;

public record BasketRequestDto(List<BasketRequestItemDto>? Items, string? Strictness);
