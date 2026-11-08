export const DEFAULT_BASKET_STRATEGY = "balanced";
export const DEFAULT_MAXIMUM_STORES = 1;
export const MAXIMUM_STORES_LIMIT = 10;

export const BASKET_STRATEGIES = Object.freeze([
  {
    value: "fastest",
    label: "Fastest",
    description: "Prioritize the shortest estimated shopping and travel time.",
  },
  {
    value: "balanced",
    label: "Balanced",
    description: "Balance estimated travel time with the available savings.",
  },
  {
    value: "cheapest",
    label: "Cheapest",
    description: "Prioritize the lowest estimated basket total across stores.",
  },
]);

const strategyValues = new Set(
  BASKET_STRATEGIES.map((strategy) => strategy.value),
);

export function parseBasketStrategy(value) {
  return strategyValues.has(value) ? value : DEFAULT_BASKET_STRATEGY;
}

export function isBasketStrategy(value) {
  return strategyValues.has(value);
}

export function parseMaximumStores(value) {
  const maximumStores = Number(value);

  return Number.isInteger(maximumStores) &&
    maximumStores >= DEFAULT_MAXIMUM_STORES &&
    maximumStores <= MAXIMUM_STORES_LIMIT
    ? maximumStores
    : DEFAULT_MAXIMUM_STORES;
}

export function isMaximumStores(value) {
  return String(parseMaximumStores(value)) === value;
}
