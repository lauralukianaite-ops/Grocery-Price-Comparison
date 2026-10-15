export const selectBasketItemCount = (state) =>
  state.items.reduce((total, item) => total + item.quantity, 0);

export const selectBasketProductCount = (state) => state.items.length;

export function getBasketEstimatedTotal(lines) {
  return lines.reduce((total, { product, quantity }) => {
    const price = Number(product.bestOffer?.cost);
    return Number.isFinite(price) ? total + price * quantity : total;
  }, 0);
}
