import assert from "node:assert/strict";
import test from "node:test";
import {
  DEFAULT_CATALOG_FILTERS,
  buildCatalogSearchParams,
  parseCatalogSearchParams,
} from "../src/features/catalog/catalogSearchParams.js";
import {
  compareCatalogProducts,
  getBestOffer,
  getDiscountPercentage,
  getProductDiscountPercentage,
} from "../src/features/catalog/catalogProducts.js";
import { getSeededProducts, getSeededStores } from "../src/features/catalog/seededProducts.js";

test("catalog defaults to highest discount", () => {
  assert.equal(parseCatalogSearchParams(new URLSearchParams()).sort, "discount-high");
});

test("filters and descending name sorting round-trip through shareable URLs", () => {
  const parameters = new URLSearchParams({
    q: "milk",
    page: "3",
    sort: "name-desc",
    stores: "2,1,2",
    minPrice: "1.20",
    maxPrice: "4",
    deals: "1",
  });
  const state = parseCatalogSearchParams(parameters);
  const url = buildCatalogSearchParams(new URLSearchParams("unrelated=keep"), state);

  assert.equal(url.get("stores"), "1,2");
  assert.equal(url.get("unrelated"), "keep");
  assert.deepEqual(parseCatalogSearchParams(url), state);
});

test("existing store-name bookmarks remain supported", () => {
  const state = parseCatalogSearchParams(new URLSearchParams("store=Rimi&sort=price-high"));
  const url = buildCatalogSearchParams(new URLSearchParams(), state);

  assert.equal(state.store, "Rimi");
  assert.equal(state.storeIds, "");
  assert.equal(url.get("store"), "Rimi");
});

test("clearing filters removes old and new filter parameters", () => {
  const parameters = new URLSearchParams("store=Rimi&stores=1&minPrice=1&maxPrice=5&deals=1&page=3");
  const cleared = buildCatalogSearchParams(parameters, {
    ...parseCatalogSearchParams(parameters),
    ...DEFAULT_CATALOG_FILTERS,
    page: 1,
  });

  for (const name of ["store", "stores", "minPrice", "maxPrice", "deals", "page"]) {
    assert.equal(cleared.has(name), false);
  }
});

test("invalid pagination, sort, and prices use safe defaults", () => {
  const state = parseCatalogSearchParams(new URLSearchParams(
    "page=-4&sort=invalid&minPrice=-1&maxPrice=Infinity",
  ));

  assert.equal(state.page, 1);
  assert.equal(state.sort, "discount-high");
  assert.equal(state.minPrice, "");
  assert.equal(state.maxPrice, "");
});

test("all sorting directions use current offers and stable ordering", () => {
  const products = [
    { id: 1, name: "Apple", offers: [{ cost: 50, retailCost: 100 }] },
    { id: 2, name: "Milk", offers: [{ cost: 1, retailCost: 1 }, { cost: 2, retailCost: 8 }] },
    { id: 3, name: "Orange", offers: [] },
    { id: 4, name: "Banana", offers: [{ cost: 2, retailCost: null }] },
  ].map((product) => ({ ...product, bestOffer: getBestOffer(product.offers) }));
  const expectedOrders = [
    ["discount-high", [2, 1, 4, 3]],
    ["discount-low", [4, 1, 2, 3]],
    ["price-low", [2, 4, 1, 3]],
    ["price-high", [1, 4, 2, 3]],
    ["name", [1, 4, 2, 3]],
    ["name-desc", [3, 2, 4, 1]],
  ];

  for (const [sort, expectedIds] of expectedOrders) {
    const sortedIds = [...products]
      .sort((left, right) => compareCatalogProducts(left, right, sort))
      .map(({ id }) => id);
    assert.deepEqual(sortedIds, expectedIds, sort);
  }

  assert.equal(getDiscountPercentage(products[1].bestOffer), 0);
  assert.equal(getProductDiscountPercentage(products[1]), 75);
});

test("the cheapest offer uses store ID to break price ties without changing the input", () => {
  const offers = [
    { storeId: 3, cost: 5 },
    { storeId: 2, cost: 2 },
    { storeId: 1, cost: 2 },
  ];
  const original = structuredClone(offers);

  assert.equal(getBestOffer(offers).storeId, 1);
  assert.deepEqual(offers, original);
  assert.equal(getBestOffer([]), null);
});

test("sample filters use only the selected stores' prices and discounts", () => {
  const products = getSeededProducts({
    storeIds: "sample-fresh-market",
    minPrice: "3",
    discountedOnly: true,
  });

  assert.equal(getSeededStores().length, 3);
  assert.equal(products.length, 1);
  assert.equal(products[0].id, "sample-free-range-eggs");
  assert.equal(products[0].bestOffer.cost, 3.99);
  assert.equal(products[0].offers.length, 1);
  assert.deepEqual(getSeededProducts({ minPrice: "100" }), []);
  assert.equal(getSeededProducts({ store: "Fresh Market" }).length, 2);
});
