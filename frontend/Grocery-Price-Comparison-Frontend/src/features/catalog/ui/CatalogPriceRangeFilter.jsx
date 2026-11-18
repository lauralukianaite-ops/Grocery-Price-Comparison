import { useState } from "react";
import { Button } from "../../../components/ui/button";
import { Input } from "../../../components/ui/input";

export default function CatalogPriceRangeFilter({ minPrice, maxPrice, onChange }) {
  const [draftMinPrice, setDraftMinPrice] = useState(minPrice);
  const [draftMaxPrice, setDraftMaxPrice] = useState(maxPrice);

  function applyPriceRange(event) {
    event.preventDefault();
    onChange({ minPrice: draftMinPrice, maxPrice: draftMaxPrice });
  }

  return (
    <form className="border-t border-border p-5" onSubmit={applyPriceRange}>
      <fieldset>
        <legend className="mb-3 text-sm font-medium">Price range (€)</legend>
        <div className="grid grid-cols-2 gap-2">
          <label className="space-y-1.5 text-xs text-muted-foreground">
            Min
            <Input
              name="minPrice"
              type="number"
              min="0"
              step="0.01"
              placeholder="0.00"
              value={draftMinPrice}
              onChange={(event) => setDraftMinPrice(event.target.value)}
              className="mt-1.5 bg-background"
            />
          </label>
          <label className="space-y-1.5 text-xs text-muted-foreground">
            Max
            <Input
              name="maxPrice"
              type="number"
              min={draftMinPrice || "0"}
              step="0.01"
              placeholder="Any"
              value={draftMaxPrice}
              onChange={(event) => setDraftMaxPrice(event.target.value)}
              className="mt-1.5 bg-background"
            />
          </label>
        </div>
        <Button type="submit" variant="outline" size="sm" className="mt-3 w-full">
          Apply price range
        </Button>
      </fieldset>
    </form>
  );
}
