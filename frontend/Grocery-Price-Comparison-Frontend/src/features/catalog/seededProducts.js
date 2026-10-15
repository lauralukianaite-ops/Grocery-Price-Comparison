const SAMPLE_HISTORY_DATES = [
  "2026-05-01",
  "2026-06-01",
  "2026-07-01",
  "2026-08-01",
  "2026-09-01",
  "2026-10-01",
];

function createSamplePriceHistory(...storeSeries) {
  return storeSeries.flatMap(({ storeId, storeName, costs }) =>
    SAMPLE_HISTORY_DATES.map((recordedAt, index) => ({
      storeId,
      storeName,
      cost: costs[index],
      recordedAt,
    })),
  );
}

const SEEDED_PRODUCTS = Object.freeze([
  {
    id: "sample-whole-milk",
    name: "Whole Milk 1L",
    imageUrl: null,
    offers: [
      {
        storeId: "sample-fresh-market",
        storeName: "Fresh Market",
        cost: 1.29,
        retailCost: 1.49,
        recordedAt: null,
      },
      {
        storeId: "sample-daily-grocer",
        storeName: "Daily Grocer",
        cost: 1.39,
        retailCost: null,
        recordedAt: null,
      },
    ],
    bestOffer: {
      storeId: "sample-fresh-market",
      storeName: "Fresh Market",
      cost: 1.29,
      retailCost: 1.49,
      recordedAt: null,
    },
    priceHistory: createSamplePriceHistory(
      {
        storeId: "sample-fresh-market",
        storeName: "Fresh Market",
        costs: [1.45, 1.42, 1.39, 1.35, 1.32, 1.29],
      },
      {
        storeId: "sample-daily-grocer",
        storeName: "Daily Grocer",
        costs: [1.52, 1.49, 1.45, 1.45, 1.42, 1.39],
      },
    ),
    lastUpdated: null,
  },
  {
    id: "sample-free-range-eggs",
    name: "Free Range Eggs 12 Pack",
    imageUrl: null,
    offers: [
      {
        storeId: "sample-family-foods",
        storeName: "Family Foods",
        cost: 3.75,
        retailCost: null,
        recordedAt: null,
      },
      {
        storeId: "sample-fresh-market",
        storeName: "Fresh Market",
        cost: 3.99,
        retailCost: 4.25,
        recordedAt: null,
      },
    ],
    bestOffer: {
      storeId: "sample-family-foods",
      storeName: "Family Foods",
      cost: 3.75,
      retailCost: null,
      recordedAt: null,
    },
    priceHistory: createSamplePriceHistory(
      {
        storeId: "sample-family-foods",
        storeName: "Family Foods",
        costs: [4.1, 4.05, 3.95, 3.89, 3.8, 3.75],
      },
      {
        storeId: "sample-fresh-market",
        storeName: "Fresh Market",
        costs: [4.35, 4.29, 4.22, 4.15, 4.05, 3.99],
      },
    ),
    lastUpdated: null,
  },
  {
    id: "sample-bananas",
    name: "Bananas 1kg",
    imageUrl: null,
    offers: [
      {
        storeId: "sample-daily-grocer",
        storeName: "Daily Grocer",
        cost: 1.84,
        retailCost: 2.05,
        recordedAt: null,
      },
      {
        storeId: "sample-family-foods",
        storeName: "Family Foods",
        cost: 1.95,
        retailCost: null,
        recordedAt: null,
      },
    ],
    bestOffer: {
      storeId: "sample-daily-grocer",
      storeName: "Daily Grocer",
      cost: 1.84,
      retailCost: 2.05,
      recordedAt: null,
    },
    priceHistory: createSamplePriceHistory(
      {
        storeId: "sample-daily-grocer",
        storeName: "Daily Grocer",
        costs: [2.15, 2.09, 2.05, 1.98, 1.9, 1.84],
      },
      {
        storeId: "sample-family-foods",
        storeName: "Family Foods",
        costs: [2.19, 2.15, 2.08, 2.05, 1.99, 1.95],
      },
    ),
    lastUpdated: null,
  },
]);

function compareByPrice(left, right) {
  return (left.bestOffer?.cost ?? Number.POSITIVE_INFINITY) -
    (right.bestOffer?.cost ?? Number.POSITIVE_INFINITY);
}

export function getSeededProducts({ query = "", sort = "price-low" } = {}) {
  const normalizedQuery = query.trim().toLocaleLowerCase();
  const products = SEEDED_PRODUCTS.filter((product) =>
    !normalizedQuery ||
    product.name.toLocaleLowerCase().includes(normalizedQuery) ||
    product.offers.some((offer) =>
      offer.storeName.toLocaleLowerCase().includes(normalizedQuery),
    ),
  );

  return [...products].sort((left, right) => {
    if (sort === "price-high") return compareByPrice(right, left);
    if (sort === "name") return left.name.localeCompare(right.name);
    return compareByPrice(left, right);
  });
}

export function getSeededProduct(productId) {
  return SEEDED_PRODUCTS.find(
    (product) => String(product.id) === String(productId),
  );
}
