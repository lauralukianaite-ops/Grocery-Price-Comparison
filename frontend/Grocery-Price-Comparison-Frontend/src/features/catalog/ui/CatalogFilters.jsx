import { useId, useState } from "react";
import { ChevronDown, SlidersHorizontal } from "lucide-react";
import { Button } from "../../../components/ui/button";
import { Skeleton } from "../../../components/ui/skeleton";
import { cn } from "../../../lib/utils";
import { parseSelectedStores } from "../catalogSearchParams";
import CatalogPriceRangeFilter from "./CatalogPriceRangeFilter";

export default function CatalogFilters({
  stores,
  selectedStores,
  isLoadingStores,
  storesError,
  onRetryStores,
  minPrice,
  maxPrice,
  discountedOnly,
  hasActiveFilters,
  onChange,
  onClear,
}) {
  const [isExpanded, setIsExpanded] = useState(false);
  const filterFieldsId = useId();
  const selectedStoreIds = parseSelectedStores(selectedStores);

  function toggleStore(id) {
    const storeId = String(id);
    const nextStoreIds = selectedStoreIds.includes(storeId)
      ? selectedStoreIds.filter((selectedId) => selectedId !== storeId)
      : [...selectedStoreIds, storeId];

    onChange({ storeIds: nextStoreIds.sort().join(",") });
  }

  return (
    <aside
      aria-label="Product filters"
      className="self-start rounded-xl border border-border bg-card lg:sticky lg:top-6"
    >
      <div className="flex items-center justify-between gap-3 p-5">
        <h2 className="flex items-center gap-2 text-sm font-semibold">
          <SlidersHorizontal className="size-4" aria-hidden="true" />
          Filters
          {hasActiveFilters && (
            <span className="size-2 rounded-full bg-emerald-600" aria-label="Filters active" />
          )}
        </h2>
        <Button
          type="button"
          size="sm"
          variant="ghost"
          className="h-7 px-2 text-xs"
          onClick={onClear}
          disabled={!hasActiveFilters}
        >
          Reset
        </Button>
        <Button
          type="button"
          size="sm"
          variant="outline"
          className="ml-auto lg:hidden"
          aria-expanded={isExpanded}
          aria-controls={filterFieldsId}
          onClick={() => setIsExpanded((expanded) => !expanded)}
        >
          {isExpanded ? "Hide" : "Show"}
          <ChevronDown
            className={cn("size-4 transition-transform", isExpanded && "rotate-180")}
            aria-hidden="true"
          />
        </Button>
      </div>
      <div
        id={filterFieldsId}
        className={cn("border-t border-border lg:block", !isExpanded && "hidden")}
      >
        <fieldset className="p-5" aria-busy={isLoadingStores}>
          <legend className="float-left mb-3 w-full text-sm font-medium">Stores</legend>
          <p className="clear-both mb-4 text-xs leading-relaxed text-muted-foreground">
            Compare prices at any of the selected stores.
          </p>
          <div className="max-h-64 space-y-3 overflow-y-auto">
            {isLoadingStores && <StoreListSkeleton />}
            {stores.map((store) => (
              <label key={store.id} className="flex cursor-pointer items-center gap-3 text-sm">
                <input
                  type="checkbox"
                  className="size-4 shrink-0 accent-foreground"
                  checked={selectedStoreIds.includes(String(store.id))}
                  onChange={() => toggleStore(store.id)}
                />
                <span>{store.name}</span>
              </label>
            ))}
            {!isLoadingStores && stores.length === 0 && (
              <p className="text-xs text-muted-foreground">
                {storesError ? "Could not load stores." : "No stores available yet."}
              </p>
            )}
          </div>
          {!isLoadingStores && storesError && (
            <Button className="mt-3" size="sm" variant="outline" onClick={onRetryStores}>
              Retry stores
            </Button>
          )}
          {stores.length > 0 && selectedStoreIds.length === 0 && (
            <p className="mt-4 text-xs text-muted-foreground">All stores included</p>
          )}
        </fieldset>
        <CatalogPriceRangeFilter
          key={`${minPrice}:${maxPrice}`}
          minPrice={minPrice}
          maxPrice={maxPrice}
          onChange={onChange}
        />
        <div className="border-t border-border p-5">
          <label className="flex cursor-pointer items-center gap-3 text-sm font-medium">
            <input
              type="checkbox"
              className="size-4 accent-foreground"
              checked={discountedOnly}
              onChange={(event) => onChange({ discountedOnly: event.target.checked })}
            />
            Discounted products only
          </label>
          <p className="mt-2 text-xs leading-relaxed text-muted-foreground">
            Price sorting uses the best price at your selected stores. Discount sorting
            uses their biggest available discount.
          </p>
        </div>
      </div>
    </aside>
  );
}

function StoreListSkeleton() {
  return (
    <div role="status">
      <span className="sr-only">Loading stores…</span>
      <div className="space-y-3" aria-hidden="true">
        {Array.from({ length: 4 }, (_, index) => (
          <div key={index} className="flex h-5 items-center gap-3">
            <Skeleton className="size-4 shrink-0 rounded-sm" />
            <Skeleton className="h-4 w-28" />
          </div>
        ))}
      </div>
    </div>
  );
}
