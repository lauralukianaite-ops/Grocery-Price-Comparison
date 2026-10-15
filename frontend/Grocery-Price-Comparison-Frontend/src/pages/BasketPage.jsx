import {
  BasketEmptyState,
  BasketHeader,
  BasketItemsList,
  BasketStrategySelector,
  BasketSummary,
  getBasketEstimatedTotal,
  selectBasketItemCount,
  useBasketProducts,
  useBasketStore,
} from "../features/basket";

export default function BasketPage() {
  const items = useBasketStore((state) => state.items);
  const itemCount = useBasketStore(selectBasketItemCount);
  const updateProductQuantity = useBasketStore(
    (state) => state.updateProductQuantity,
  );
  const removeProduct = useBasketStore((state) => state.removeProduct);
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
      className="mx-auto max-w-6xl px-4 py-10 sm:px-6 sm:py-14 lg:px-8"
    >
      <BasketHeader
        itemCount={itemCount}
        productCount={items.length}
        onClearBasket={clearBasket}
      />

      <BasketStrategySelector />

      <div className="mt-7 grid gap-7 lg:grid-cols-[minmax(0,1fr)_20rem]">
        <BasketItemsList
          lines={basketProducts.lines}
          isLoading={basketProducts.isLoading}
          error={basketProducts.error}
          unavailableProductCount={basketProducts.unavailableProductCount}
          onRetry={basketProducts.retry}
          onQuantityChange={updateProductQuantity}
          onRemove={removeProduct}
        />
        <BasketSummary
          itemCount={itemCount}
          estimatedTotal={estimatedTotal}
        />
      </div>
    </main>
  );
}
