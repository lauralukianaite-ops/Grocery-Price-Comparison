import { get } from "../api/apiClient";

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

function normalizeProduct(item) {
  const offers = (item.offers ?? item.prices ?? [])
    .map(normalizeOffer)
    .filter((offer) => Number.isFinite(offer.cost));
  const bestOffer = offers.reduce(
    (best, offer) => (!best || offer.cost < best.cost ? offer : best),
    null,
  );

  return {
    id: item.id,
    name: item.name,
    imageUrl: item.imageUrl ?? null,
    offers,
    bestOffer,
    lastUpdated: offers.reduce((latest, offer) => {
      if (!offer.recordedAt) return latest;
      return !latest || offer.recordedAt > latest ? offer.recordedAt : latest;
    }, null),
  };
}

export async function getItems(
  { page = 1, pageSize = 12, query = "", sort = "price-low" },
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

  const response = await get(`/items?${searchParams.toString()}`, { signal });
  const items = Array.isArray(response) ? response : (response.items ?? []);

  return {
    items: items.map(normalizeProduct),
    totalCount: Array.isArray(response)
      ? response.length
      : (response.totalCount ?? items.length),
    page: Array.isArray(response) ? page : (response.page ?? page),
    pageSize: Array.isArray(response)
      ? pageSize
      : (response.pageSize ?? pageSize),
  };
}
