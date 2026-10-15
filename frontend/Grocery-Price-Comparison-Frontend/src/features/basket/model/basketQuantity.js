export const MIN_BASKET_QUANTITY = 1;
export const MAX_BASKET_QUANTITY = 99;

export function normalizeBasketQuantity(value, fallback = MIN_BASKET_QUANTITY) {
  const parsedValue = Number(value);

  if (!Number.isFinite(parsedValue)) {
    return fallback;
  }

  return Math.min(
    MAX_BASKET_QUANTITY,
    Math.max(MIN_BASKET_QUANTITY, Math.trunc(parsedValue)),
  );
}
