export const DEFAULT_CATALOG_STATE = Object.freeze({
  page: 1,
  query: "",
  sort: "price-low",
});

const CATALOG_PARAM_NAMES = Object.freeze({
  page: "page",
  query: "q",
  sort: "sort",
});

const SUPPORTED_SORTS = new Set(["price-low", "price-high", "name"]);

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
    sort: SUPPORTED_SORTS.has(requestedSort)
      ? requestedSort
      : DEFAULT_CATALOG_STATE.sort,
  };
}

export function buildCatalogSearchParams(currentSearchParams, catalogState) {
  const nextSearchParams = new URLSearchParams(currentSearchParams);
  const query = catalogState.query.trim();

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
