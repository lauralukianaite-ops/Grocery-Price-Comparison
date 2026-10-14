import { X } from "lucide-react";
import { Link } from "react-router-dom";
import { Button } from "../../../components/ui/button";
import { Card, CardContent } from "../../../components/ui/card";
import { getProductRoute } from "../../../router/routes";
import { formatPrice, ProductVisual } from "../../catalog";
import QuantitySelector from "./QuantitySelector";

export default function BasketLineItem({
  product,
  quantity,
  onQuantityChange,
  onRemove,
}) {
  const unitPrice = Number(product.bestOffer?.cost);
  const lineTotal = Number.isFinite(unitPrice) ? unitPrice * quantity : null;

  return (
    <Card className="p-3">
      <CardContent className="flex gap-4 p-0">
        <Link
          className="w-24 shrink-0 rounded-xl outline-none focus-visible:ring-3 focus-visible:ring-ring/50 sm:w-32"
          to={getProductRoute(product.id)}
          state={{ product }}
          aria-label={`View ${product.name}`}
        >
          <ProductVisual product={product} />
        </Link>
        <div className="min-w-0 flex-1 py-1">
          <div className="flex items-start justify-between gap-2">
            <div className="min-w-0">
              <Link
                className="font-semibold hover:underline"
                to={getProductRoute(product.id)}
                state={{ product }}
              >
                {product.name}
              </Link>
              <p className="mt-1 text-xs text-muted-foreground">
                {product.bestOffer?.storeName ?? "No store price available"}
              </p>
            </div>
            <Button
              size="icon"
              variant="ghost"
              onClick={onRemove}
              aria-label={`Remove ${product.name} from basket`}
            >
              <X aria-hidden="true" />
            </Button>
          </div>
          <div className="mt-4 flex flex-wrap items-end justify-between gap-3">
            <div>
              <p className="text-lg font-semibold">{formatPrice(lineTotal)}</p>
              {quantity > 1 && Number.isFinite(unitPrice) && (
                <p className="mt-0.5 text-xs text-muted-foreground">
                  {formatPrice(unitPrice)} each
                </p>
              )}
            </div>
            <QuantitySelector
              value={quantity}
              onChange={onQuantityChange}
              productName={product.name}
            />
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
