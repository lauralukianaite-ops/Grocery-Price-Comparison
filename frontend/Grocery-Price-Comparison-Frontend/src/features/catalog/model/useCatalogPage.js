import { useEffect } from "react";
import { useSearchParams } from "react-router-dom";
import { useLocationStore } from "../../location";
import {
  buildCatalogSearchParams,
  parseCatalogSearchParams,
} from "../catalogSearchParams";
import { getSeededProducts } from "../seededProducts";
import { useProducts } from "./useProducts";

export const CATALOG_PAGE_SIZE = 12;
export const CATALOG_SORT_OPTIONS = Object.freeze([
  { value: "price-low", label: "Price: low to high" },
  { value: "price-high", label: "Price: high to low" },
  { value: "name", label: "Product name" },
]);

export function useCatalogPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const { page, query, sort } = parseCatalogSearchParams(searchParams);
  const currentLocation = useLocationStore((state) => state.currentLocation);
  const { items, totalCount, isLoading, error, retry } = useProducts({
    page,
    pageSize: CATALOG_PAGE_SIZE,
    query,
    sort,
    latitude: currentLocation?.latitude,
    longitude: currentLocation?.longitude,
  });
  const isUsingSeededProducts =
    !isLoading && (Boolean(error) || items.length === 0);
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
      sort,
    });

    if (canonicalSearchParams.toString() !== searchParams.toString()) {
      setSearchParams(canonicalSearchParams, { replace: true });
    }
  }, [page, query, searchParams, setSearchParams, sort]);

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
    updateCatalogState(
      { query: nextQuery, page: 1 },
      { replace: true },
    );
  }

  function updateSort(nextSort) {
    updateCatalogState({ sort: nextSort, page: 1 });
  }

  function changePage(nextPage) {
    updateCatalogState({ page: Math.min(Math.max(nextPage, 1), pageCount) });
    document.getElementById("products")?.scrollIntoView({ behavior: "smooth" });
  }

  return {
    page,
    pageCount,
    query,
    sort,
    items: visibleItems,
    totalCount: visibleTotalCount,
    isLoading,
    isUsingSeededProducts,
    retry,
    updateQuery,
    updateSort,
    changePage,
  };
}
