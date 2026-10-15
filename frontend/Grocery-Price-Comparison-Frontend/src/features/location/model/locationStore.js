import { create } from "zustand";
import { persist } from "zustand/middleware";

export const useLocationStore = create(
  persist(
    (set) => ({
      currentLocation: null,
      saveCurrentLocation: (currentLocation) => set({ currentLocation }),
      clearCurrentLocation: () => set({ currentLocation: null }),
    }),
    {
      name: "pricebasket-location",
      version: 1,
      partialize: (state) => ({ currentLocation: state.currentLocation }),
    },
  ),
);
