import { useId } from "react";
import { ArrowDownWideNarrow, Search } from "lucide-react";
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
  sort,
  isLoading,
  totalCount,
  onQueryChange,
  onSortChange,
}) {
  const sortLabelId = useId();
  const selectedSortLabel = CATALOG_SORT_OPTIONS.find((option) => option.value === sort)?.label;
  const productLabel = totalCount === 1 ? "product" : "products";
  const resultsLabel = isLoading
    ? "Finding products…"
    : totalCount.toLocaleString() + " " + productLabel + " found";

  return (
<<<<<<< HEAD
<<<<<<< HEAD
    <div className="mb-6 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
      <p className="hidden text-sm text-muted-foreground sm:block"></p>
      <div className="flex min-w-0 flex-1 gap-2 sm:max-w-xl sm:justify-end">
=======
    <div className="mb-6 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-end">
      <div className="flex min-w-0 flex-1 gap-2 sm:max-w-2xl sm:justify-end">
>>>>>>> c65371a148dfa35ce79aaeb3f3115c12556897d3
        <label className="relative min-w-0 flex-1 sm:max-w-xs">
=======
    <div className="mb-6 flex flex-col gap-4">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <p className="text-sm text-muted-foreground">Find your next basket item</p>
        <p role="status" className="text-sm text-muted-foreground">{resultsLabel}</p>
      </div>
      <div className="flex min-w-0 flex-col gap-3 sm:flex-row sm:items-end">
        <label className="relative min-w-0 flex-1">
>>>>>>> feature/additional-sorting
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
        <div className="space-y-1.5">
          <span id={sortLabelId} className="flex items-center gap-1.5 text-xs font-medium text-muted-foreground">
            <ArrowDownWideNarrow className="size-3.5" aria-hidden="true" />
            Sort by
          </span>
          <Select value={sort} onValueChange={onSortChange}>
            <SelectTrigger aria-labelledby={sortLabelId} className="h-9 w-full bg-background sm:w-[14rem]">
              <SelectValue>{selectedSortLabel}</SelectValue>
            </SelectTrigger>
            <SelectContent align="end">
              {CATALOG_SORT_OPTIONS.map((option) => (
                <SelectItem key={option.value} value={option.value}>{option.label}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </div>
    </div>
  );
}
