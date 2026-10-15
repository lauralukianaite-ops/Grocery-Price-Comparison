import { useCallback, useEffect, useState } from "react";
import { getCatalog } from "../api/catalogApi";

const EMPTY_CATALOG = {
  items: [],
  totalCount: 0,
  page: 1,
  pageSize: 12,
};

export function useProducts({
  page,
  pageSize,
  query,
  sort,
  latitude,
  longitude,
}) {
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
        const nextCatalog = await getCatalog(
          { page, pageSize, query, sort, latitude, longitude },
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
  }, [latitude, longitude, page, pageSize, query, requestVersion, sort]);

  return { ...catalog, isLoading, error, retry };
}
