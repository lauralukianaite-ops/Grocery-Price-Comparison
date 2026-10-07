import { get } from "../api/apiClient";

export function getItemPriceHistory(itemId) {
  return get(`/items/${itemId}/price-history`);
}
