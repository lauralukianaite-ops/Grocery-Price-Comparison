import { useEffect, useState } from "react";
import { getStores } from "../api/catalogApi";

export function useStores() {
  const [stores, setStores] = useState([]);

  useEffect(() => {
    const controller = new AbortController();

    getStores(controller.signal)
      .then(setStores)
      .catch((err) => {
        if (err.name !== "AbortError") {
          setStores([]);
        }
      });

    return () => controller.abort();
  }, []);

  return stores;
}