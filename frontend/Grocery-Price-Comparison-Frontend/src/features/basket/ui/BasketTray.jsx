import { ShoppingBasket } from "lucide-react";
import { Button } from "../../../components/ui/button";
import { formatPrice } from "../../catalog/productFormatting";

export default function BasketTray({ lines, itemCount, estimatedTotal }) {
  return (
    <aside className="sticky bottom-3 z-20 mt-5 w-full rounded-xl border border-primary/20 bg-background/95 p-3 shadow-xl backdrop-blur supports-backdrop-filter:bg-background/85" aria-label="Basket tray">
      <div className="flex flex-col gap-3 lg:flex-row lg:items-center">
        <div className="flex items-center gap-2 lg:w-36">
          <span className="grid size-9 place-items-center rounded-lg bg-primary text-primary-foreground"><ShoppingBasket className="size-4" aria-hidden="true" /></span>
          <div><p className="text-sm font-semibold">Basket tray</p><p className="text-xs text-muted-foreground">{itemCount} items</p></div>
        </div>
        <div className="flex min-w-0 flex-1 gap-2 overflow-x-auto pb-1 lg:pb-0">
          {lines.slice(0, 4).map(({ product, quantity }) => (
            <div key={product.id} className="min-w-40 rounded-lg bg-muted/60 px-3 py-2">
              <p className="truncate text-xs font-medium">{product.name}</p>
              <p className="mt-0.5 text-xs text-muted-foreground">{quantity} × {formatPrice(product.bestOffer?.cost)}</p>
            </div>
          ))}
        </div>
        <div className="flex items-center justify-between gap-3 border-t pt-3 lg:border-t-0 lg:border-l lg:pl-4 lg:pt-0">
          <div><p className="text-xs text-muted-foreground">Optimal split total</p><p className="text-lg font-semibold tabular-nums">{formatPrice(estimatedTotal)}</p></div>
          <Button type="button">Proceed to route</Button>
        </div>
      </div>
    </aside>
  );
}
