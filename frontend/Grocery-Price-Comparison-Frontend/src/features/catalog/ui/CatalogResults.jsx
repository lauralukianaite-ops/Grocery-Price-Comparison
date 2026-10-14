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
  query,
  isLoading,
  isUsingSeededProducts,
  onRetry,
  onClearSearch,
  renderProductActions,
}) {
  if (isLoading) {
    return <ProductGridSkeleton />;
  }

  return (
    <>
      {isUsingSeededProducts && (
        <Alert className="mb-5 py-4">
          <FlaskConical aria-hidden="true" />
          <AlertTitle>Showing sample products</AlertTitle>
          <AlertDescription>
            The catalog API is not returning products yet, so these temporary
            examples are shown instead.
          </AlertDescription>
          <AlertAction>
            <Button size="sm" variant="outline" onClick={onRetry}>
              Retry
            </Button>
          </AlertAction>
        </Alert>
      )}

      {items.length > 0 ? (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
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
              Try another search or clear the current search term.
            </p>
            {query && (
              <Button className="mt-5" variant="outline" onClick={onClearSearch}>
                Clear search
              </Button>
            )}
          </CardContent>
        </Card>
      )}
    </>
  );
}
