import { FlaskConical, Sparkles } from "lucide-react";
import { Badge } from "../../../components/ui/badge";

export default function CatalogHeader({
  isLoading,
  isUsingSeededProducts,
  totalCount,
}) {
  return (
    <div className="flex flex-col gap-6 border-b border-border pb-8 sm:flex-row sm:items-end sm:justify-between">
      <div>
        <Badge variant="outline" className="mb-4 gap-1.5">
          {isUsingSeededProducts ? (
            <FlaskConical aria-hidden="true" />
          ) : (
            <Sparkles aria-hidden="true" />
          )}
          {isUsingSeededProducts ? "Sample catalog" : "Live store data"}
        </Badge>
        <h1 className="text-3xl font-semibold tracking-[-0.035em] sm:text-4xl">
          All products
        </h1>
        <p className="mt-3 max-w-xl text-sm leading-6 text-muted-foreground sm:text-base">
          Browse groceries and compare their latest available prices across
          participating stores.
        </p>
      </div>
      <p className="text-sm text-muted-foreground" aria-live="polite">
        <span className="font-semibold text-foreground">
          {isLoading ? "—" : totalCount}
        </span>{" "}
        products found
      </p>
    </div>
  );
}
