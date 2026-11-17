import { useId, useState } from "react";
import { ChevronDown, SlidersHorizontal } from "lucide-react";
import { Button } from "../../../components/ui/button";
import { cn } from "../../../lib/utils";
import { parseSelectedStores } from "../catalogSearchParams";
import CatalogPriceRangeFilter from "./CatalogPriceRangeFilter";

export default function CatalogFilters({
  stores,
  selectedStores,
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
        <fieldset className="p-5">
          <legend className="float-left mb-3 w-full text-sm font-medium">Stores</legend>
          <p className="clear-both mb-4 text-xs leading-relaxed text-muted-foreground">
            Compare prices at any of the selected stores.
          </p>
          <div className="max-h-64 space-y-3 overflow-y-auto">
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
            {stores.length === 0 && (
              <p className="text-xs text-muted-foreground">
                No stores available yet.
              </p>
            )}
          </div>
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
