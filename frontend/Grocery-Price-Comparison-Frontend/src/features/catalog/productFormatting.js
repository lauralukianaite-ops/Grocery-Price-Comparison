const priceFormatter = new Intl.NumberFormat("en", {
  style: "currency",
  currency: "EUR",
});

const dateFormatter = new Intl.DateTimeFormat("en", {
  day: "numeric",
  month: "short",
});

export function formatPrice(price) {
  return Number.isFinite(Number(price)) ? priceFormatter.format(Number(price)) : "—";
}

export function formatUpdatedAt(value) {
  if (!value) return "Update time unavailable";

  const date = new Date(value);

  return Number.isNaN(date.getTime())
    ? "Update time unavailable"
    : `Updated ${dateFormatter.format(date)}`;
}
