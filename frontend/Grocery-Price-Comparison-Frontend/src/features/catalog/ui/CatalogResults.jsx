import { FlaskConical, PackageSearch } from "lucide-react";
import {
  Alert,
  AlertAction,
  AlertDescription,
  AlertTitle,
} from "../../../components/ui/alert";
import { Button } from "../../../components/ui/button";
import { Card, CardContent } from "../../../components/ui/card";
import ProductCard from "./ProductCard";
import ProductGridSkeleton from "./ProductGridSkeleton";

export default function CatalogResults({
  items,
  hasSearchOrFilters,
  isLoading,
  isUsingSeededProducts,
  error,
  onRetry,
  onClearSearchAndFilters,
  renderProductActions,
}) {
  if (isLoading) {
    return <ProductGridSkeleton />;
  }

  if (error && !isUsingSeededProducts) {
    return (
      <Alert className="py-4">
        <PackageSearch aria-hidden="true" />
        <AlertTitle>Could not load products</AlertTitle>
        <AlertDescription>Please try again to load current store prices.</AlertDescription>
        <AlertAction>
          <Button size="sm" variant="outline" onClick={onRetry}>Retry</Button>
        </AlertAction>
      </Alert>
    );
  }

  return (
    <>
      {isUsingSeededProducts && (
        <Alert className="mb-5 py-4">
          <FlaskConical aria-hidden="true" />
          <AlertTitle>Showing sample products</AlertTitle>
          <AlertDescription>
            The live catalog is temporarily unavailable. Browse these sample
            products or retry to load current store prices.
          </AlertDescription>
          <AlertAction>
            <Button size="sm" variant="outline" onClick={onRetry}>
              Retry
            </Button>
          </AlertAction>
        </Alert>
      )}

      {items.length > 0 ? (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {items.map((product) => (
            <ProductCard
              key={product.id}
              product={product}
              actions={renderProductActions?.(product)}
            />
          ))}
        </div>
      ) : (
        <Card className="border-dashed py-16 text-center">
          <CardContent>
            <PackageSearch className="mx-auto size-10 text-muted-foreground" />
            <h2 className="mt-4 text-lg font-semibold">No products found</h2>
            <p className="mt-2 text-sm text-muted-foreground">
              Try another search, choose different stores, or widen your price range.
            </p>
            {hasSearchOrFilters && (
              <Button className="mt-5" variant="outline" onClick={onClearSearchAndFilters}>
                Clear search and filters
              </Button>
            )}
          </CardContent>
        </Card>
      )}
    </>
  );
}
