import { Clock3, Store } from "lucide-react";
import { Link } from "react-router-dom";
import { getProductRoute } from "../../../router/routes";
import { Badge } from "../../../components/ui/badge";
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from "../../../components/ui/card";
import { formatPrice, formatUpdatedAt } from "../productFormatting";
import ProductVisual from "./ProductVisual";

export default function ProductCard({ product, actions }) {
  const { bestOffer } = product;
  const hasDiscount =
    bestOffer?.retailCost && bestOffer.retailCost > bestOffer.cost;

  return (
    <Card className="flex h-full flex-col gap-0 p-2 transition-shadow hover:shadow-md">
      <Link
        className="block flex-1 rounded-xl outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
        to={getProductRoute(product.id)}
        state={{ product }}
        aria-label={`View ${product.name}`}
      >
        <ProductVisual product={product} />
        <CardHeader className="gap-2 px-3 pt-4 pb-3">
          <div className="flex items-start justify-between gap-3">
            <div className="min-w-0">
              <CardTitle className="line-clamp-2 text-[0.98rem]">
                {product.name}
              </CardTitle>
              <p className="mt-1 flex items-center gap-1 text-xs text-muted-foreground">
                <Clock3 className="size-3" aria-hidden="true" />
                {formatUpdatedAt(product.lastUpdated)}
              </p>
            </div>
            {hasDiscount && (
              <Badge className="bg-emerald-100 text-emerald-800">Deal</Badge>
            )}
          </div>
        </CardHeader>
        <CardContent className="px-3 pb-4">
          <div className="flex items-end justify-between gap-3">
            <div>
              <p className="text-xs text-muted-foreground">Best price</p>
              <div className="mt-0.5 flex items-baseline gap-2">
                <span className="text-xl font-semibold tracking-tight">
                  {bestOffer ? formatPrice(bestOffer.cost) : "—"}
                </span>
                {hasDiscount && (
                  <span className="text-xs text-muted-foreground line-through">
                    {formatPrice(bestOffer.retailCost)}
                  </span>
                )}
              </div>
            </div>
            <div className="text-right text-xs text-muted-foreground">
              <p className="font-medium text-foreground">
                {bestOffer?.storeName ?? "No prices yet"}
              </p>
              <p className="mt-1 flex items-center justify-end gap-1">
                <Store className="size-3" aria-hidden="true" />
                {product.offers.length}{" "}
                {product.offers.length === 1 ? "store" : "stores"}
              </p>
            </div>
          </div>
        </CardContent>
      </Link>
      <CardFooter className="border-0 bg-transparent p-3 pt-0">
        {actions}
      </CardFooter>
    </Card>
  );
}
