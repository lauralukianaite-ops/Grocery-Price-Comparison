import {
  BasketEmptyState,
  BasketHeader,
  BasketComparisonPreview,
  BasketStrategySelector,
  BasketTray,
  getBasketEstimatedTotal,
  selectBasketItemCount,
  useBasketProducts,
  useBasketStore,
} from "../features/basket";

export default function BasketPage() {
  const items = useBasketStore((state) => state.items);
  const itemCount = useBasketStore(selectBasketItemCount);
  const clearBasket = useBasketStore((state) => state.clearBasket);
  const basketProducts = useBasketProducts(items);
  const estimatedTotal = getBasketEstimatedTotal(basketProducts.lines);

  if (items.length === 0) {
    return <BasketEmptyState />;
  }

  return (
    <main
      id="main-content"
      tabIndex="-1"
      className="mx-auto min-h-screen w-full max-w-[var(--page-content-width)] px-4 py-6 sm:px-6 lg:px-8 xl:px-10"
    >
      <BasketHeader
        itemCount={itemCount}
        productCount={items.length}
        onClearBasket={clearBasket}
      />

      <BasketStrategySelector />

      <BasketComparisonPreview />

      <BasketTray
        lines={basketProducts.lines}
        itemCount={itemCount}
        estimatedTotal={estimatedTotal}
      />
    </main>
  );
}
