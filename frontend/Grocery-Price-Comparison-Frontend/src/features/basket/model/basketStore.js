import { create } from "zustand";
import { persist } from "zustand/middleware";
import { normalizeBasketQuantity } from "./basketQuantity";

function getProductId(productOrId) {
  const productId =
    typeof productOrId === "object" && productOrId !== null
      ? productOrId.id
      : productOrId;

  return productId === undefined || productId === null
    ? null
    : String(productId);
}

function normalizeBasketItems(items) {
  if (!Array.isArray(items)) {
    return [];
  }

  const normalizedItems = new Map();

  items.forEach((item) => {
    const isBasketLine =
      Object.prototype.hasOwnProperty.call(item ?? {}, "productId") ||
      Boolean(item?.product);
    const productId = getProductId(
      item?.productId ?? item?.product ?? item,
    );

    if (!productId) {
      return;
    }

    const quantity = isBasketLine
      ? normalizeBasketQuantity(item.quantity)
      : 1;
    const existingItem = normalizedItems.get(productId);

    normalizedItems.set(productId, {
      productId,
      quantity: normalizeBasketQuantity(
        (existingItem?.quantity ?? 0) + quantity,
      ),
    });
  });

  return [...normalizedItems.values()];
}

export const useBasketStore = create(
  persist(
    (set) => ({
      items: [],
      addProduct: (productOrId, quantity = 1) =>
        set((state) => {
          const productId = getProductId(productOrId);

          if (!productId) {
            return state;
          }

          const normalizedQuantity = normalizeBasketQuantity(quantity);
          const existingItemIndex = state.items.findIndex(
            (item) => item.productId === productId,
          );

          if (existingItemIndex === -1) {
            return {
              items: [...state.items, { productId, quantity: normalizedQuantity }],
            };
          }

          return {
            items: state.items.map((item, index) =>
              index === existingItemIndex
                ? { productId, quantity: normalizedQuantity }
                : item,
            ),
          };
        }),
      updateProductQuantity: (productOrId, quantity) =>
        set((state) => {
          const productId = getProductId(productOrId);

          return {
            items: state.items.map((item) =>
              item.productId === productId
                ? {
                    ...item,
                    quantity: normalizeBasketQuantity(quantity, item.quantity),
                  }
                : item,
            ),
          };
        }),
      removeProduct: (productOrId) =>
        set((state) => {
          const productId = getProductId(productOrId);

          return {
            items: state.items.filter((item) => item.productId !== productId),
          };
        }),
      clearBasket: () => set({ items: [] }),
    }),
    {
      name: "pricebasket-basket",
      version: 3,
      migrate: (persistedState) => ({
        ...persistedState,
        items: normalizeBasketItems(persistedState?.items),
      }),
      partialize: (state) => ({ items: state.items }),
    },
  ),
);
