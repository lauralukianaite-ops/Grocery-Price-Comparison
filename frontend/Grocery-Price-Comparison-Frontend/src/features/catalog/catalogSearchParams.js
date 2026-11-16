export const DEFAULT_CATALOG_STATE = Object.freeze({
  page: 1,
  query: "",
  store: "",
  sort: "discount-high",
});

const CATALOG_PARAM_NAMES = Object.freeze({
  page: "page",
  query: "q",
  store: "store",
  sort: "sort",
});

const SUPPORTED_SORTS = new Set(["discount-high", "price-low", "price-high", "name"]);

function parsePage(value) {
  const page = Number(value);
  return Number.isSafeInteger(page) && page > 0
    ? page
    : DEFAULT_CATALOG_STATE.page;
}

export function parseCatalogSearchParams(searchParams) {
  const requestedSort = searchParams.get(CATALOG_PARAM_NAMES.sort);

  return {
    page: parsePage(searchParams.get(CATALOG_PARAM_NAMES.page)),
    query: searchParams.get(CATALOG_PARAM_NAMES.query) ?? "",
    store: searchParams.get(CATALOG_PARAM_NAME.store) ?? "",
    sort: SUPPORTED_SORTS.has(requestedSort)
      ? requestedSort
      : DEFAULT_CATALOG_STATE.sort,
  };
}

export function buildCatalogSearchParams(currentSearchParams, catalogState) {
  const nextSearchParams = new URLSearchParams(currentSearchParams);
  const query = catalogState.query.trim();

  const store = (catalogState.store ?? "").trim();
  if (store) {
    nextSearchParams.set(CATALOG_PARAM_NAMES.store, store);
  } else {
    nextSearchParams.delete(CATALOG_PARAM_NAMES.store);
  }

  if (query) {
    nextSearchParams.set(CATALOG_PARAM_NAMES.query, query);
  } else {
    nextSearchParams.delete(CATALOG_PARAM_NAMES.query);
  }

  if (catalogState.sort === DEFAULT_CATALOG_STATE.sort) {
    nextSearchParams.delete(CATALOG_PARAM_NAMES.sort);
  } else {
    nextSearchParams.set(CATALOG_PARAM_NAMES.sort, catalogState.sort);
  }

  if (catalogState.page === DEFAULT_CATALOG_STATE.page) {
    nextSearchParams.delete(CATALOG_PARAM_NAMES.page);
  } else {
    nextSearchParams.set(CATALOG_PARAM_NAMES.page, String(catalogState.page));
  }

  return nextSearchParams;
}
