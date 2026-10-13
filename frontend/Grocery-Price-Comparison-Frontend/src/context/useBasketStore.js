import { create } from "zustand";

export const useBasketStore = create((set) => ({
  productIds: [],
  toggleProduct: (productId) =>
    set((state) => ({
      productIds: state.productIds.includes(productId)
        ? state.productIds.filter((id) => id !== productId)
        : [...state.productIds, productId],
    })),
}));
