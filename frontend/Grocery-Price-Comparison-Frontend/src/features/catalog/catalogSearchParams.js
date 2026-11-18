import { CATALOG_SORT_OPTIONS } from "./catalogConstants.js";

export const DEFAULT_CATALOG_FILTERS = Object.freeze({
  store: "",
  storeIds: "",
  minPrice: "",
  maxPrice: "",
  discountedOnly: false,
});

export const DEFAULT_CATALOG_STATE = Object.freeze({
  page: 1,
  query: "",
  sort: "discount-high",
  ...DEFAULT_CATALOG_FILTERS,
});

const SUPPORTED_SORTS = new Set(CATALOG_SORT_OPTIONS.map(({ value }) => value));

function parsePage(value) {
  const page = Number(value);
  return Number.isSafeInteger(page) && page > 0 ? page : DEFAULT_CATALOG_STATE.page;
}

function parsePrice(value) {
  if (!value?.trim()) return "";

  const price = Number(value);
  return Number.isFinite(price) && price >= 0 ? String(price) : "";
}

export function parseSelectedStores(value = "") {
  const storeIds = value.split(",").map((storeId) => storeId.trim()).filter(Boolean);
  return [...new Set(storeIds)].sort();
}

export function parseCatalogSearchParams(searchParams) {
  const requestedSort = searchParams.get("sort");

  return {
    page: parsePage(searchParams.get("page")),
    query: searchParams.get("q") ?? "",
    store: searchParams.get("store") ?? "",
    storeIds: parseSelectedStores(searchParams.get("stores") ?? "").join(","),
    sort: SUPPORTED_SORTS.has(requestedSort) ? requestedSort : DEFAULT_CATALOG_STATE.sort,
    minPrice: parsePrice(searchParams.get("minPrice")),
    maxPrice: parsePrice(searchParams.get("maxPrice")),
    discountedOnly: searchParams.get("deals") === "1",
  };
}

export function buildCatalogSearchParams(currentSearchParams, catalogState) {
  const nextSearchParams = new URLSearchParams(currentSearchParams);
  const parameters = {
    q: catalogState.query,
    store: catalogState.store.trim(),
    stores: catalogState.storeIds,
    sort: catalogState.sort === DEFAULT_CATALOG_STATE.sort ? "" : catalogState.sort,
    page: catalogState.page === DEFAULT_CATALOG_STATE.page ? "" : String(catalogState.page),
    minPrice: catalogState.minPrice,
    maxPrice: catalogState.maxPrice,
    deals: catalogState.discountedOnly ? "1" : "",
  };

  for (const [name, value] of Object.entries(parameters)) {
    if (value) {
      nextSearchParams.set(name, value);
    } else {
      nextSearchParams.delete(name);
    }
  }

  return nextSearchParams;
}
