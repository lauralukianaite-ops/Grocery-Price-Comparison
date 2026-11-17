import { useEffect } from "react";
import { useSearchParams } from "react-router-dom";
import { useLocationStore } from "../../location";
import {
  buildCatalogSearchParams,
  parseCatalogSearchParams,
} from "../catalogSearchParams";
import { CATALOG_PAGE_SIZE } from "../catalogConstants";
import { getSeededProducts } from "../seededProducts";
import { useProducts } from "./useProducts";
import { useStores } from "./useStores";

export function useCatalogPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const { page, query, store, sort } = parseCatalogSearchParams(searchParams);
  const stores = useStores();
  const currentLocation = useLocationStore((state) => state.currentLocation);
  const { items, totalCount, isLoading, error, retry } = useProducts({
    page,
    pageSize: CATALOG_PAGE_SIZE,
    query,
    store,
    sort,
    latitude: currentLocation?.latitude,
    longitude: currentLocation?.longitude,
  });

  const isUsingSeededProducts =
    import.meta.env.DEV &&
    !isLoading &&
    (Boolean(error) || (!store && !query && items.length === 0));
  const visibleItems = isUsingSeededProducts
    ? getSeededProducts({ query, sort })
    : items;
  const visibleTotalCount = isUsingSeededProducts
    ? visibleItems.length
    : totalCount;
  const pageCount = isUsingSeededProducts
    ? 1
    : Math.max(1, Math.ceil(totalCount / CATALOG_PAGE_SIZE));

  useEffect(() => {
    const canonicalSearchParams = buildCatalogSearchParams(searchParams, {
      page,
      query,
      store,
      sort,
    });

    if (canonicalSearchParams.toString() !== searchParams.toString()) {
      setSearchParams(canonicalSearchParams, { replace: true });
    }
  }, [page, query, searchParams, setSearchParams, sort, store]);

  useEffect(() => {
    if (!isLoading && page > pageCount) {
      setSearchParams(
        (currentSearchParams) =>
          buildCatalogSearchParams(currentSearchParams, {
            ...parseCatalogSearchParams(currentSearchParams),
            page: pageCount,
          }),
        { replace: true },
      );
    }
  }, [isLoading, page, pageCount, setSearchParams]);

  function updateCatalogState(updates, navigationOptions) {
    setSearchParams(
      (currentSearchParams) =>
        buildCatalogSearchParams(currentSearchParams, {
          ...parseCatalogSearchParams(currentSearchParams),
          ...updates,
        }),
      navigationOptions,
    );
  }

  function updateQuery(nextQuery) {
    updateCatalogState({ query: nextQuery, page: 1 }, { replace: true });
  }

  function updateSort(nextSort) {
    updateCatalogState({ sort: nextSort, page: 1 });
  }

  function updateStore(nextStore) {
    updateCatalogState({ store: nextStore, page: 1 });
  }

  function changePage(nextPage) {
    updateCatalogState({ page: Math.min(Math.max(nextPage, 1), pageCount) });
  }

  return {
    page,
    pageCount,
    query,
    store,
    stores,
    sort,
    items: visibleItems,
    totalCount: visibleTotalCount,
    isLoading,
    error,
    isUsingSeededProducts,
    retry,
    updateQuery,
    updateSort,
    updateStore,
    changePage,
  };
}