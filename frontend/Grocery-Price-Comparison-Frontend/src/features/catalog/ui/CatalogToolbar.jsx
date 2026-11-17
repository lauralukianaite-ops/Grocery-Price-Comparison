import { Search } from "lucide-react";
import { Input } from "../../../components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "../../../components/ui/select";
import { CATALOG_SORT_OPTIONS } from "../catalogConstants";

export default function CatalogToolbar({
  query,
  store,
  stores = [],
  sort,
  isLoading,
  onQueryChange,
  onStoreChange,
  onSortChange,
}) {
  return (
    <div className="mb-6 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-end">
      <div className="flex min-w-0 flex-1 gap-2 sm:max-w-2xl sm:justify-end">
        <label className="relative min-w-0 flex-1 sm:max-w-xs">
          <span className="sr-only">Search all products</span>
          <Search className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            className="h-9 bg-background pl-9"
            type="search"
            placeholder="Search products"
            value={query}
            onChange={(event) => onQueryChange(event.target.value)}
            aria-busy={isLoading}
          />
        </label>

        <Select
          value={store || "all"}
          onValueChange={(value) => onStoreChange(value === "all" ? "" : value)}
        >
          <SelectTrigger className="h-9 w-[11rem] bg-background">
            <SelectValue>{store || "All stores"}</SelectValue>
          </SelectTrigger>
          <SelectContent align="end">
            <SelectItem value="all">All stores</SelectItem>
            {stores.map((storeOption) => (
              <SelectItem key={storeOption.id} value={storeOption.name}>
                {storeOption.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Select value={sort} onValueChange={onSortChange}>
          <SelectTrigger className="h-9 w-[13.5rem] bg-background">
            <SelectValue>
              {
                CATALOG_SORT_OPTIONS.find((option) => option.value === sort)
                  ?.label
              }
            </SelectValue>
          </SelectTrigger>
          <SelectContent align="end">
            {CATALOG_SORT_OPTIONS.map((option) => (
              <SelectItem key={option.value} value={option.value}>
                {option.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
    </div>
  );
}
