import { useEffect } from "react";
import { AlertCircle, PackageSearch, Search, Sparkles } from "lucide-react";
import { useSearchParams } from "react-router-dom";
import ProductCard from "../components/products/ProductCard";
import ProductGridSkeleton from "../components/products/ProductGridSkeleton";
import {
  Alert,
  AlertAction,
  AlertDescription,
  AlertTitle,
} from "../components/ui/alert";
import { Badge } from "../components/ui/badge";
import { Button } from "../components/ui/button";
import { Card, CardContent } from "../components/ui/card";
import { Input } from "../components/ui/input";
import {
  Pagination,
  PaginationContent,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from "../components/ui/pagination";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "../components/ui/select";
import { useBasketStore } from "../context/useBasketStore";
import {
  buildCatalogSearchParams,
  parseCatalogSearchParams,
} from "../features/catalog/catalogSearchParams";
import { useProducts } from "../hooks/useProducts";

const PAGE_SIZE = 12;

export default function ProductsPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const { page, query, sort } = parseCatalogSearchParams(searchParams);
  const basketProductIds = useBasketStore((state) => state.productIds);
  const toggleProduct = useBasketStore((state) => state.toggleProduct);
  const { items, totalCount, isLoading, error, retry } = useProducts({
    page,
    pageSize: PAGE_SIZE,
    query,
    sort,
  });
  const pageCount = Math.max(1, Math.ceil(totalCount / PAGE_SIZE));

  useEffect(() => {
    const canonicalSearchParams = buildCatalogSearchParams(searchParams, {
      page,
      query,
      sort,
    });

    if (canonicalSearchParams.toString() !== searchParams.toString()) {
      setSearchParams(canonicalSearchParams, { replace: true });
    }
  }, [page, query, searchParams, setSearchParams, sort]);

  useEffect(() => {
    if (!isLoading && !error && page > pageCount) {
      setSearchParams(
        (currentSearchParams) =>
          buildCatalogSearchParams(currentSearchParams, {
            ...parseCatalogSearchParams(currentSearchParams),
            page: pageCount,
          }),
        { replace: true },
      );
    }
  }, [error, isLoading, page, pageCount, setSearchParams]);

  function updateCatalogState(updates, navigationOptions) {
    setSearchParams(
      (currentSearchParams) =>
        buildCatalogSearchParams(currentSearchParams, {
          ...parseCatalogSearchParams(currentSearchParams),
          ...updates,
        }),
      navigationOptions,
    );
  }

  function updateQuery(nextQuery) {
    updateCatalogState(
      { query: nextQuery, page: 1 },
      { replace: true },
    );
  }

  function updateSort(nextSort) {
    updateCatalogState({ sort: nextSort, page: 1 });
  }

  function changePage(event, nextPage) {
    event.preventDefault();
    updateCatalogState({ page: Math.min(Math.max(nextPage, 1), pageCount) });
    document.getElementById("products")?.scrollIntoView({ behavior: "smooth" });
  }

  return (
    <main
      id="main-content"
      tabIndex="-1"
      className="mx-auto max-w-7xl px-4 py-10 sm:px-6 sm:py-14 lg:px-8"
    >
        <div className="flex flex-col gap-6 border-b border-border pb-8 sm:flex-row sm:items-end sm:justify-between">
          <div>
            <Badge variant="outline" className="mb-4 gap-1.5">
              <Sparkles aria-hidden="true" /> Live store data
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

        <section id="products" className="mt-8" aria-label="Product catalog">
          <div className="mb-6 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <p className="hidden text-sm text-muted-foreground sm:block">
              Prices and availability come directly from the catalog API.
            </p>
            <div className="flex min-w-0 flex-1 gap-2 sm:max-w-xl sm:justify-end">
              <label className="relative min-w-0 flex-1 sm:max-w-xs">
                <span className="sr-only">Search all products</span>
                <Search className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted-foreground" />
                <Input
                  className="h-9 bg-background pl-9"
                  type="search"
                  placeholder="Search products"
                  value={query}
                  onChange={(event) => updateQuery(event.target.value)}
                  aria-busy={isLoading}
                />
              </label>
              <Select value={sort} onValueChange={updateSort}>
                <SelectTrigger className="h-9 w-[11.5rem] bg-background">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent align="end">
                  <SelectItem value="price-low">Price: low to high</SelectItem>
                  <SelectItem value="price-high">Price: high to low</SelectItem>
                  <SelectItem value="name">Product name</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>

          {isLoading && <ProductGridSkeleton />}

          {!isLoading && error && (
            <Alert variant="destructive" className="py-4">
              <AlertCircle aria-hidden="true" />
              <AlertTitle>Products could not be loaded</AlertTitle>
              <AlertDescription>{error}</AlertDescription>
              <AlertAction>
                <Button size="sm" variant="outline" onClick={retry}>
                  Retry
                </Button>
              </AlertAction>
            </Alert>
          )}

          {!isLoading && !error && items.length > 0 && (
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
              {items.map((product) => (
                <ProductCard
                  key={product.id}
                  product={product}
                  isAdded={basketProductIds.includes(product.id)}
                  onToggleBasket={toggleProduct}
                />
              ))}
            </div>
          )}

          {!isLoading && !error && items.length === 0 && (
            <Card className="border-dashed py-16 text-center">
              <CardContent>
                <PackageSearch className="mx-auto size-10 text-muted-foreground" />
                <h2 className="mt-4 text-lg font-semibold">No products found</h2>
                <p className="mt-2 text-sm text-muted-foreground">
                  Try another search or clear the current search term.
                </p>
                {query && (
                  <Button
                    className="mt-5"
                    variant="outline"
                    onClick={() => updateQuery("")}
                  >
                    Clear search
                  </Button>
                )}
              </CardContent>
            </Card>
          )}

          {!isLoading && !error && pageCount > 1 && (
            <Pagination className="mt-10">
              <PaginationContent>
                <PaginationItem>
                  <PaginationPrevious
                    href="#products"
                    aria-disabled={page === 1}
                    className={
                      page === 1 ? "pointer-events-none opacity-40" : ""
                    }
                    onClick={(event) => changePage(event, page - 1)}
                  />
                </PaginationItem>
                {Array.from(
                  { length: pageCount },
                  (_, index) => index + 1,
                ).map((pageNumber) => (
                  <PaginationItem key={pageNumber}>
                    <PaginationLink
                      href="#products"
                      isActive={page === pageNumber}
                      onClick={(event) => changePage(event, pageNumber)}
                    >
                      {pageNumber}
                    </PaginationLink>
                  </PaginationItem>
                ))}
                <PaginationItem>
                  <PaginationNext
                    href="#products"
                    aria-disabled={page === pageCount}
                    className={
                      page === pageCount
                        ? "pointer-events-none opacity-40"
                        : ""
                    }
                    onClick={(event) => changePage(event, page + 1)}
                  />
                </PaginationItem>
              </PaginationContent>
            </Pagination>
          )}
        </section>
    </main>
  );
}
