import { useState } from "react";
import { searchItems } from "../services/items/searchItems";

export function useItemSearch() {
  const [items, setItems] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");
  const [hasSearched, setHasSearched] = useState(false);

  async function search(itemName) {
    setItems([]);
    setIsLoading(true);
    setError("");
    setHasSearched(true);

    try {
      const similarItems = await searchItems(itemName);
      setItems(similarItems);
    } catch (requestError) {
      setError(requestError.message);
    } finally {
      setIsLoading(false);
    }
  }

  return { search, items, isLoading, error, hasSearched };
}
