import { useState } from "react";
import { Plus, X } from "lucide-react";
import { cn } from "../../../lib/utils";
import { Button } from "../../../components/ui/button";
import { useBasketStore } from "../model/basketStore";
import QuantitySelector from "./QuantitySelector";

export default function BasketProductControls({
  product,
  buttonSize = "default",
  className,
}) {
  const basketLine = useBasketStore((state) =>
    state.items.find(
      (item) => item.productId === String(product.id),
    ),
  );
  const addProduct = useBasketStore((state) => state.addProduct);
  const updateProductQuantity = useBasketStore(
    (state) => state.updateProductQuantity,
  );
  const removeProduct = useBasketStore((state) => state.removeProduct);
  const [selectedQuantity, setSelectedQuantity] = useState(
    basketLine?.quantity ?? 1,
  );
  const isAdded = Boolean(basketLine);
  const displayedQuantity = basketLine?.quantity ?? selectedQuantity;

  function handleQuantityChange(quantity) {
    setSelectedQuantity(quantity);

    if (isAdded) {
      updateProductQuantity(product.id, quantity);
    }
  }

  function handleBasketAction() {
    if (isAdded) {
      removeProduct(product.id);
      setSelectedQuantity(1);
      return;
    }

    addProduct(product, selectedQuantity);
  }

  return (
    <div className={cn("flex w-full flex-col gap-3", className)}>
      <div className="flex items-center justify-between gap-3">
        <span className="text-sm font-medium">Quantity</span>
        <QuantitySelector
          value={displayedQuantity}
          onChange={handleQuantityChange}
          productName={product.name}
        />
      </div>
      <Button
        className="w-full"
        size={buttonSize}
        variant={isAdded ? "outline" : "default"}
        onClick={handleBasketAction}
      >
        {isAdded ? <X aria-hidden="true" /> : <Plus aria-hidden="true" />}
        {isAdded ? "Remove from basket" : "Add to basket"}
      </Button>
    </div>
  );
}
