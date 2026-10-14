import { AlertCircle, LoaderCircle } from "lucide-react";
import {
  Alert,
  AlertAction,
  AlertDescription,
  AlertTitle,
} from "../../../components/ui/alert";
import { Button } from "../../../components/ui/button";
import BasketLineItem from "./BasketLineItem";

export default function BasketItemsList({
  lines,
  isLoading,
  error,
  unavailableProductCount,
  onRetry,
  onQuantityChange,
  onRemove,
}) {
  return (
    <section aria-label="Basket products" className="space-y-3">
      {isLoading && (
        <Alert>
          <LoaderCircle className="animate-spin" aria-hidden="true" />
          <AlertTitle>Refreshing basket prices</AlertTitle>
          <AlertDescription>
            Loading the latest product and store information.
          </AlertDescription>
        </Alert>
      )}

      {error && (
        <Alert variant="destructive">
          <AlertCircle aria-hidden="true" />
          <AlertTitle>Some basket products could not be refreshed</AlertTitle>
          <AlertDescription>{error}</AlertDescription>
          <AlertAction>
            <Button size="sm" variant="outline" onClick={onRetry}>
              Retry
            </Button>
          </AlertAction>
        </Alert>
      )}

      {!isLoading && unavailableProductCount > 0 && !error && (
        <Alert>
          <AlertCircle aria-hidden="true" />
          <AlertTitle>Some products are currently unavailable</AlertTitle>
          <AlertDescription>
            {unavailableProductCount} saved product
            {unavailableProductCount === 1 ? "" : "s"} could not be found in
            the latest catalog response.
          </AlertDescription>
        </Alert>
      )}

      {lines.map(({ product, productId, quantity }) => (
        <BasketLineItem
          key={productId}
          product={product}
          quantity={quantity}
          onQuantityChange={(nextQuantity) =>
            onQuantityChange(productId, nextQuantity)
          }
          onRemove={() => onRemove(productId)}
        />
      ))}
    </section>
  );
}
