import { useCallback, useEffect, useMemo, useState } from "react";
import { getCatalog } from "../api/catalogApi";
import { CATALOG_PAGE_SIZE } from "../catalogConstants";

const EMPTY_CATALOG = { items: [], totalCount: 0, page: 1, pageSize: CATALOG_PAGE_SIZE };
const SEARCH_DEBOUNCE_MS = 300;

export function useProducts({
  page,
  pageSize,
  query,
  store,
  storeIds,
  sort,
  minPrice,
  maxPrice,
  discountedOnly,
  latitude,
  longitude,
}) {
  const [result, setResult] = useState({
    catalog: EMPTY_CATALOG,
    request: null,
    requestVersion: null,
    error: "",
  });
  const [requestVersion, setRequestVersion] = useState(0);
  const request = useMemo(
    () => ({
      page,
      pageSize,
      query,
      store,
      storeIds,
      sort,
      minPrice,
      maxPrice,
      discountedOnly,
      latitude,
      longitude,
    }),
    [
      page,
      pageSize,
      query,
      store,
      storeIds,
      sort,
      minPrice,
      maxPrice,
      discountedOnly,
      latitude,
      longitude,
    ],
  );
  const retry = useCallback(() => setRequestVersion((current) => current + 1), []);

  useEffect(() => {
    const controller = new AbortController();

    async function loadCatalog() {
      try {
        const catalog = await getCatalog(request, controller.signal);
        if (!controller.signal.aborted) {
          setResult({ catalog, request, requestVersion, error: "" });
        }
      } catch (error) {
        if (!controller.signal.aborted && error.name !== "AbortError") {
          setResult({
            catalog: {
              ...EMPTY_CATALOG,
              page: request.page,
              pageSize: request.pageSize,
            },
            request,
            requestVersion,
            error: error.message,
          });
        }
      }
    }

    const requestDelay = setTimeout(loadCatalog, request.query ? SEARCH_DEBOUNCE_MS : 0);
    return () => {
      clearTimeout(requestDelay);
      controller.abort();
    };
  }, [request, requestVersion]);

  // Mark changed requests as loading before their debounce timer runs.
  const isLoading = result.request !== request || result.requestVersion !== requestVersion;
  return { ...result.catalog, isLoading, error: result.error, retry };
}
