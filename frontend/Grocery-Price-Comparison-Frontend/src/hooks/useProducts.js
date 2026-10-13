import { useCallback, useEffect, useState } from "react";
import { getItems } from "../services/items/getItems";

const EMPTY_CATALOG = {
  items: [],
  totalCount: 0,
  page: 1,
  pageSize: 12,
};

export function useProducts({ page, pageSize, query, sort }) {
  const [catalog, setCatalog] = useState(EMPTY_CATALOG);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState("");
  const [requestVersion, setRequestVersion] = useState(0);

  const retry = useCallback(() => {
    setRequestVersion((current) => current + 1);
  }, []);

  useEffect(() => {
    const controller = new AbortController();
    const requestDelay = setTimeout(async () => {
      setIsLoading(true);
      setError("");

      try {
        const nextCatalog = await getItems(
          { page, pageSize, query, sort },
          controller.signal,
        );
        setCatalog(nextCatalog);
      } catch (requestError) {
        if (requestError.name !== "AbortError") {
          setCatalog({ ...EMPTY_CATALOG, page, pageSize });
          setError(requestError.message);
        }
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false);
        }
      }
    }, query ? 300 : 0);

    return () => {
      clearTimeout(requestDelay);
      controller.abort();
    };
  }, [page, pageSize, query, requestVersion, sort]);

  return { ...catalog, isLoading, error, retry };
}
