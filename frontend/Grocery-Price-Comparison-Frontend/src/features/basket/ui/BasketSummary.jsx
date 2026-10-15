import { Link } from "react-router-dom";
import { buttonVariants } from "../../../components/ui/button";
import { Card, CardContent } from "../../../components/ui/card";
import { Separator } from "../../../components/ui/separator";
import { cn } from "../../../lib/utils";
import { APP_ROUTES } from "../../../router/routes";
import { formatPrice } from "../../catalog";

export default function BasketSummary({ itemCount, estimatedTotal }) {
  return (
    <aside aria-label="Basket summary">
      <Card className="sticky top-24">
        <CardContent>
          <h2 className="font-semibold">Price summary</h2>
          <div className="mt-4 flex justify-between text-sm text-muted-foreground">
            <span>Best prices</span>
            <span>
              {itemCount} {itemCount === 1 ? "item" : "items"}
            </span>
          </div>
          <Separator className="my-4" />
          <div className="flex items-baseline justify-between gap-4">
            <span className="font-medium">Estimated total</span>
            <span className="text-2xl font-semibold">
              {formatPrice(estimatedTotal)}
            </span>
          </div>
          <p className="mt-3 text-xs leading-5 text-muted-foreground">
            Calculated from each quantity and its product&apos;s current best
            listed price.
          </p>
          <Link
            className={cn(
              buttonVariants({ variant: "outline" }),
              "mt-5 w-full",
            )}
            to={APP_ROUTES.products}
          >
            Continue browsing
          </Link>
        </CardContent>
      </Card>
    </aside>
  );
}
