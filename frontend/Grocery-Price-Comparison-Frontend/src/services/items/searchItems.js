import { get } from "../api/apiClient";

export async function searchItems(itemName, threshold) {
  const searchParams = new URLSearchParams();

  if (threshold !== undefined) {
    searchParams.set("threshold", threshold);
  }

  const queryString = searchParams.toString();
  const response = await get(
    `/items/search/${encodeURIComponent(itemName)}${
      queryString ? `?${queryString}` : ""
    }`,
  );

  return response.similarItems;
}
