import { get } from "../api/apiClient";

export function getItemDiscountCheck(itemId) {
  return get(`/items/${itemId}/discount-check`);
}
