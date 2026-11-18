import { useCallback, useEffect, useState } from "react";
import { getStores } from "../api/catalogApi";

export function useStores() {
  const [result, setResult] = useState({ stores: [], error: "", requestVersion: null });
  const [requestVersion, setRequestVersion] = useState(0);
  const retry = useCallback(() => setRequestVersion((current) => current + 1), []);

  useEffect(() => {
    const controller = new AbortController();

    async function loadStores() {
      try {
        const stores = await getStores(controller.signal);
        if (!controller.signal.aborted) {
          setResult({ stores, error: "", requestVersion });
        }
      } catch (error) {
        if (!controller.signal.aborted && error.name !== "AbortError") {
          setResult({ stores: [], error: error.message, requestVersion });
        }
      }
    }

    loadStores();
    return () => controller.abort();
  }, [requestVersion]);

  return {
    stores: result.stores,
    error: result.error,
    isLoading: result.requestVersion !== requestVersion,
    retry,
  };
}
