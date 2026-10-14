import { Minus, Plus } from "lucide-react";
import {
  MAX_BASKET_QUANTITY,
  MIN_BASKET_QUANTITY,
  normalizeBasketQuantity,
} from "../model/basketQuantity";
import { cn } from "../../../lib/utils";
import { Button } from "../../../components/ui/button";
import { ButtonGroup } from "../../../components/ui/button-group";
import { Input } from "../../../components/ui/input";

export default function QuantitySelector({
  value,
  onChange,
  productName,
  className,
}) {
  const accessibleName = productName
    ? `Quantity for ${productName}`
    : "Quantity";

  function commitQuantity(nextValue) {
    const normalizedValue = normalizeBasketQuantity(nextValue, value);

    if (normalizedValue !== value) {
      onChange(normalizedValue);
    }
  }

  function handleInputChange(event) {
    const nextValue = event.target.value;

    if (nextValue !== "") {
      commitQuantity(nextValue);
    }
  }

  return (
    <ButtonGroup className={cn("shrink-0", className)}>
      <Button
        type="button"
        size="icon"
        variant="outline"
        disabled={value <= MIN_BASKET_QUANTITY}
        onClick={() => commitQuantity(value - 1)}
        aria-label={`Decrease ${accessibleName.toLocaleLowerCase()}`}
      >
        <Minus aria-hidden="true" />
      </Button>
      <Input
        className="h-8 w-12 rounded-none px-1 text-center tabular-nums [appearance:textfield] [&::-webkit-inner-spin-button]:appearance-none [&::-webkit-outer-spin-button]:appearance-none"
        type="number"
        inputMode="numeric"
        min={MIN_BASKET_QUANTITY}
        max={MAX_BASKET_QUANTITY}
        value={value}
        onChange={handleInputChange}
        onKeyDown={(event) => {
          if (event.key === "Enter") {
            event.currentTarget.blur();
          }
        }}
        aria-label={accessibleName}
      />
      <Button
        type="button"
        size="icon"
        variant="outline"
        disabled={value >= MAX_BASKET_QUANTITY}
        onClick={() => commitQuantity(value + 1)}
        aria-label={`Increase ${accessibleName.toLocaleLowerCase()}`}
      >
        <Plus aria-hidden="true" />
      </Button>
    </ButtonGroup>
  );
}
