export const DEFAULT_BASKET_STRATEGY = "balanced";

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
