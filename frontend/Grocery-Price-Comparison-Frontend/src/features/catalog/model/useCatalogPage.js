import { useEffect } from "react";
import { useSearchParams } from "react-router-dom";
import { useLocationStore } from "../../location";
import {
  buildCatalogSearchParams,
  DEFAULT_CATALOG_FILTERS,
  DEFAULT_CATALOG_STATE,
  parseCatalogSearchParams,
} from "../catalogSearchParams";
import { CATALOG_PAGE_SIZE } from "../catalogConstants";
import { getSeededProducts, getSeededStores } from "../seededProducts";
import { useProducts } from "./useProducts";
import { useStores } from "./useStores";

export function useCatalogPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const catalogState = parseCatalogSearchParams(searchParams);
  const { page, sort, store, storeIds, minPrice, maxPrice, discountedOnly } = catalogState;
  const stores = useStores();
  const currentLocation = useLocationStore((state) => state.currentLocation);
  const { items, totalCount, isLoading, error, retry } = useProducts({
    ...catalogState,
    pageSize: CATALOG_PAGE_SIZE,
    latitude: currentLocation?.latitude,
    longitude: currentLocation?.longitude,
  });

  // Sample products remain a development-only fallback for failed requests.
  const canUseSeededProducts = import.meta.env.DEV && Boolean(error);
  const isUsingSeededProducts = !isLoading && canUseSeededProducts;
  const availableStores = canUseSeededProducts ? getSeededStores() : stores;
  const visibleItems = isUsingSeededProducts ? getSeededProducts(catalogState) : items;
  const visibleTotalCount = isUsingSeededProducts ? visibleItems.length : totalCount;
  const pageCount = isUsingSeededProducts
    ? 1
    : Math.max(1, Math.ceil(totalCount / CATALOG_PAGE_SIZE));

  // Existing store-name bookmarks still show their selected checkbox.
  const legacyStore = availableStores.find((option) =>
    option.name.toLocaleLowerCase() === store.trim().toLocaleLowerCase(),
  );
  const selectedStoreIds = storeIds || (legacyStore ? String(legacyStore.id) : "");

  useEffect(() => {
    const canonicalSearchParams = buildCatalogSearchParams(
      searchParams,
      parseCatalogSearchParams(searchParams),
    );
    if (canonicalSearchParams.toString() !== searchParams.toString()) {
      setSearchParams(canonicalSearchParams, { replace: true });
    }
  }, [searchParams, setSearchParams]);

  useEffect(() => {
    if (!isLoading && page > pageCount) {
      setSearchParams(
        (currentSearchParams) => buildCatalogSearchParams(currentSearchParams, {
          ...parseCatalogSearchParams(currentSearchParams),
          page: pageCount,
        }),
        { replace: true },
      );
    }
  }, [isLoading, page, pageCount, setSearchParams]);

  function updateCatalogState(updates, navigationOptions) {
    setSearchParams(
      (currentSearchParams) => buildCatalogSearchParams(currentSearchParams, {
        ...parseCatalogSearchParams(currentSearchParams),
        ...updates,
      }),
      navigationOptions,
    );
  }

  function updateQuery(query) {
    updateCatalogState({ query, page: 1 }, { replace: true });
  }

  function updateSort(sort) {
    updateCatalogState({ sort, page: 1 });
  }

  function updateFilters(filters) {
    // Checkbox selections replace the legacy store-name filter.
    const storeUpdates = filters.storeIds === undefined ? {} : { store: "" };
    updateCatalogState({ ...storeUpdates, ...filters, page: 1 });
  }

  function clearFilters() {
    updateFilters(DEFAULT_CATALOG_FILTERS);
  }

  function clearSearchAndFilters() {
    updateCatalogState({ ...DEFAULT_CATALOG_STATE, sort });
  }

  function changePage(nextPage) {
    updateCatalogState({ page: Math.min(Math.max(nextPage, 1), pageCount) });
  }

  return {
    ...catalogState,
    pageCount,
    availableStores,
    selectedStoreIds,
    hasActiveFilters: Boolean(store || storeIds || minPrice || maxPrice || discountedOnly),
    items: visibleItems,
    totalCount: visibleTotalCount,
    isLoading,
    error,
    isUsingSeededProducts,
    retry,
    updateQuery,
    updateSort,
    updateFilters,
    clearFilters,
    clearSearchAndFilters,
    changePage,
  };
}
