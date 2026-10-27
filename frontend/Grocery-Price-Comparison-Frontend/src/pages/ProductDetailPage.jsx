import { ArrowLeft, Clock3, Store } from "lucide-react";
import { Link, useLocation, useParams } from "react-router-dom";
import { Badge } from "../components/ui/badge";
import { buttonVariants } from "../components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "../components/ui/card";
import { Separator } from "../components/ui/separator";
import { BasketProductControls } from "../features/basket";
import {
  formatPrice,
  formatUpdatedAt,
  getSeededProduct,
  ProductPriceHistoryChart,
  ProductVisual,
} from "../features/catalog";
import { APP_ROUTES } from "../router/routes";
import { cn } from "../lib/utils";

function resolveProduct(productId, routeProduct) {
  const requestedId = String(productId);

  if (routeProduct && String(routeProduct.id) === requestedId) {
    return routeProduct;
  }

  return getSeededProduct(requestedId);
}

export default function ProductDetailPage() {
  const { productId } = useParams();
  const { state } = useLocation();
  const product = resolveProduct(productId, state?.product);

  if (!product) {
    return (
      <main
        id="main-content"
        tabIndex="-1"
        className="mx-auto w-full max-w-[var(--page-content-width)] px-4 py-16 text-center sm:px-6 lg:px-8 xl:px-10"
      >
        <h1 className="text-3xl font-semibold tracking-tight">Product not found</h1>
        <p className="mt-3 text-muted-foreground">
          This product is not available in the current catalog.
        </p>
        <Link
          className={cn(buttonVariants(), "mt-6")}
          to={APP_ROUTES.products}
        >
          Browse all products
        </Link>
      </main>
    );
  }

  const offers = product.offers ?? [];

  return (
    <main
      id="main-content"
      tabIndex="-1"
      className="mx-auto w-full max-w-[var(--page-content-width)] px-4 py-8 sm:px-6 sm:py-12 lg:px-8 xl:px-10"
    >
      <Link
        className={cn(buttonVariants({ variant: "ghost" }), "mb-6")}
        to={APP_ROUTES.products}
      >
        <ArrowLeft aria-hidden="true" />
        All products
      </Link>

      <div className="grid gap-8 lg:grid-cols-[minmax(0,1fr)_minmax(22rem,0.8fr)]">
        <div>
          <ProductVisual product={product} />
        </div>

        <section aria-labelledby="product-name" className="flex flex-col">
          <div className="flex flex-wrap items-center gap-2">
            <Badge variant="outline">Product</Badge>
            <span className="flex items-center gap-1 text-xs text-muted-foreground">
              <Clock3 className="size-3" aria-hidden="true" />
              {formatUpdatedAt(product.lastUpdated)}
            </span>
          </div>
          <h1
            id="product-name"
            className="mt-4 text-3xl font-semibold tracking-[-0.035em] sm:text-4xl"
          >
            {product.name}
          </h1>

          <div className="mt-7 rounded-xl border bg-muted/30 p-5">
            <p className="text-sm text-muted-foreground">Best available price</p>
            <p className="mt-1 text-3xl font-semibold tracking-tight">
              {formatPrice(product.bestOffer?.cost)}
            </p>
            <p className="mt-2 flex items-center gap-1.5 text-sm">
              <Store className="size-4 text-muted-foreground" aria-hidden="true" />
              {product.bestOffer?.storeName ?? "No store prices available"}
            </p>
          </div>

          <BasketProductControls
            className="mt-5"
            product={product}
            buttonSize="lg"
          />
        </section>
      </div>

      <Separator className="my-10" />

      <section aria-labelledby="store-prices">
        <div className="flex items-end justify-between gap-4">
          <div>
            <h2 id="store-prices" className="text-xl font-semibold">
              Store prices
            </h2>
            <p className="mt-1 text-sm text-muted-foreground">
              Compare every available offer for this product.
            </p>
          </div>
          <Badge variant="secondary">
            {offers.length} {offers.length === 1 ? "store" : "stores"}
          </Badge>
        </div>

        {offers.length > 0 ? (
          <div className="mt-5 grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {offers.map((offer) => (
              <Card key={`${offer.storeId}-${offer.cost}`}>
                <CardHeader>
                  <CardTitle className="text-base">{offer.storeName}</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="flex items-baseline gap-2">
                    <span className="text-2xl font-semibold">
                      {formatPrice(offer.cost)}
                    </span>
                    {offer.retailCost > offer.cost && (
                      <span className="text-sm text-muted-foreground line-through">
                        {formatPrice(offer.retailCost)}
                      </span>
                    )}
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        ) : (
          <Card className="mt-5 border-dashed">
            <CardContent className="py-10 text-center text-sm text-muted-foreground">
              No store prices are available yet.
            </CardContent>
          </Card>
        )}
      </section>

      <ProductPriceHistoryChart priceHistory={product.priceHistory} />
    </main>
  );
}
