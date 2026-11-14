using backend.Enums;

namespace backend.DTOs;

// What CreateAsync answers: which outcome happened, and the basket when one was created.
public record BasketResult(
    BasketOutcome Outcome,
    BasketResponseDto? Basket
);
