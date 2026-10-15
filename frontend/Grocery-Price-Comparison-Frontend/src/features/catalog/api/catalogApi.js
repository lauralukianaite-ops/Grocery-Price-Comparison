import { get } from "../../../shared/api/apiClient";

function normalizeOffer(price) {
  return {
    storeId: price.storeId ?? price.store?.id,
    storeName: price.storeName ?? price.store?.name ?? "Unknown store",
    cost: Number(price.cost),
    retailCost:
      price.retailCost === null || price.retailCost === undefined
        ? null
        : Number(price.retailCost),
    recordedAt: price.recordedAt ?? null,
  };
}

function normalizePriceHistoryPoint(point) {
  return {
    storeId: point.storeId ?? point.store?.id,
    storeName: point.storeName ?? point.store?.name ?? "Unknown store",
    cost: Number(point.cost ?? point.price),
    recordedAt: point.recordedAt ?? point.date ?? point.timestamp ?? null,
  };
}

function normalizeProduct(item) {
  const offers = (item.offers ?? item.prices ?? [])
    .map(normalizeOffer)
    .filter((offer) => Number.isFinite(offer.cost));
  const bestOffer = offers.reduce(
    (best, offer) => (!best || offer.cost < best.cost ? offer : best),
    null,
  );
  const explicitPriceHistory =
    item.priceHistory ?? item.priceHistoryPoints ?? item.history ?? [];
  const priceHistory = explicitPriceHistory
    .map(normalizePriceHistoryPoint)
    .filter(
      (point) => Number.isFinite(point.cost) && Boolean(point.recordedAt),
    );

  return {
    id: item.id,
    name: item.name,
    imageUrl: item.imageUrl ?? null,
    offers,
    bestOffer,
    priceHistory:
      priceHistory.length > 0
        ? priceHistory
        : offers.filter((offer) => Boolean(offer.recordedAt)),
    lastUpdated: offers.reduce((latest, offer) => {
      if (!offer.recordedAt) return latest;
      return !latest || offer.recordedAt > latest ? offer.recordedAt : latest;
    }, null),
  };
}

function getResponseItems(response) {
  return Array.isArray(response) ? response : (response?.items ?? []);
}

export async function getCatalog(
  {
    page = 1,
    pageSize = 12,
    query = "",
    sort = "price-low",
    latitude,
    longitude,
  },
  signal,
) {
  const searchParams = new URLSearchParams({
    page: String(page),
    pageSize: String(pageSize),
    sort,
  });

  if (query.trim()) {
    searchParams.set("query", query.trim());
  }

  if (Number.isFinite(latitude) && Number.isFinite(longitude)) {
    searchParams.set("latitude", String(latitude));
    searchParams.set("longitude", String(longitude));
  }

  const response = await get(`/items?${searchParams.toString()}`, { signal });
  const items = getResponseItems(response);

  return {
    items: items.map(normalizeProduct),
    totalCount: Array.isArray(response)
      ? response.length
      : (response?.totalCount ?? items.length),
    page: Array.isArray(response) ? page : (response?.page ?? page),
    pageSize: Array.isArray(response)
      ? pageSize
      : (response?.pageSize ?? pageSize),
  };
}

export async function getProductsByIds(productIds, signal) {
  const normalizedIds = [...new Set(productIds.map(String))];

  if (normalizedIds.length === 0) {
    return [];
  }

  const searchParams = new URLSearchParams({ ids: normalizedIds.join(",") });
  const response = await get(`/items?${searchParams.toString()}`, { signal });
  const requestedIds = new Set(normalizedIds);

  return getResponseItems(response)
    .map(normalizeProduct)
    .filter((product) => requestedIds.has(String(product.id)));
}

export async function searchItems(itemName, threshold) {
  const searchParams = new URLSearchParams();

  if (threshold !== undefined) {
    searchParams.set("threshold", threshold);
  }

  const queryString = searchParams.toString();
  const response = await get(
    `/items/search/${encodeURIComponent(itemName)}${
      queryString ? `?${queryString}` : ""
    }`,
  );

  return response.similarItems;
}

export function getItemPriceHistory(itemId) {
  return get(`/items/${encodeURIComponent(String(itemId))}/price-history`);
}

export function getItemDiscountCheck(itemId) {
  return get(`/items/${encodeURIComponent(String(itemId))}/discount-check`);
}
