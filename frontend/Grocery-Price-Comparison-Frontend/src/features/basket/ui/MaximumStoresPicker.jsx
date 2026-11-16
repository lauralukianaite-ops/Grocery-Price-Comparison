import { Input } from "../../../components/ui/input";
import {
  DEFAULT_MAXIMUM_STORES,
  MAXIMUM_STORES_LIMIT,
} from "../model/basketStrategies";

function isValidMaximumStores(value) {
  return Number.isInteger(value) && value >= DEFAULT_MAXIMUM_STORES && value <= MAXIMUM_STORES_LIMIT;
}

export default function MaximumStoresPicker({ maximumStores, onChange }) {
  function handleChange(event) {
    const nextMaximumStores = Number(event.currentTarget.value);
    if (isValidMaximumStores(nextMaximumStores) && nextMaximumStores !== maximumStores) {
      onChange(nextMaximumStores);
    }
  }

  function handleBlur(event) {
    const nextMaximumStores = Number(event.currentTarget.value);
    event.currentTarget.value = String(
      isValidMaximumStores(nextMaximumStores) ? nextMaximumStores : maximumStores,
    );
  }

  return (
    <div className="flex items-center gap-2">
      <Input
        key={maximumStores}
        type="number"
        min={DEFAULT_MAXIMUM_STORES}
        max={MAXIMUM_STORES_LIMIT}
        step="1"
        inputMode="numeric"
        aria-label="Maximum stores to visit"
        className="h-9 w-20 text-center font-semibold tabular-nums"
        defaultValue={maximumStores}
        onChange={handleChange}
        onBlur={handleBlur}
      />
      <span className="text-sm text-muted-foreground">
        {maximumStores === 1 ? "store" : "stores"}
      </span>
    </div>
  );
}
