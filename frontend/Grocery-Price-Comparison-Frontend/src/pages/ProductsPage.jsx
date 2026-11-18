import { BasketProductControls } from "../features/basket";
import {
  CatalogFilters,
  CatalogHeader,
  CatalogPagination,
  CatalogResults,
  CatalogToolbar,
  useCatalogPage,
} from "../features/catalog";
import { LocationPreference } from "../features/location";

export default function ProductsPage() {
  const catalog = useCatalogPage();

  function handlePageChange(nextPage) {
    catalog.changePage(nextPage);
    document.getElementById("products")?.scrollIntoView({ behavior: "smooth" });
  }

  return (
    <main id="main-content" tabIndex="-1">
      <div className="mx-auto w-full max-w-[var(--page-content-width)] px-4 py-10 sm:px-6 sm:py-14 lg:px-8 xl:px-10">
        <CatalogHeader
          isUsingSeededProducts={catalog.isUsingSeededProducts}
        />
        <LocationPreference />

        <section
          id="products"
          className="mt-8 grid items-start gap-6 lg:grid-cols-[15rem_minmax(0,1fr)] lg:gap-8"
          aria-label="Product catalog"
        >
          <CatalogFilters
            stores={catalog.availableStores}
            selectedStores={catalog.selectedStoreIds}
            isLoadingStores={catalog.isLoadingStores}
            storesError={catalog.storesError}
            onRetryStores={catalog.retryStores}
            minPrice={catalog.minPrice}
            maxPrice={catalog.maxPrice}
            discountedOnly={catalog.discountedOnly}
            hasActiveFilters={catalog.hasActiveFilters}
            onChange={catalog.updateFilters}
            onClear={catalog.clearFilters}
          />
          <div className="min-w-0">
            <CatalogToolbar
              query={catalog.query}
              sort={catalog.sort}
              isLoading={catalog.isLoading}
              totalCount={catalog.totalCount}
              onQueryChange={catalog.updateQuery}
              onSortChange={catalog.updateSort}
            />
            <CatalogResults
              items={catalog.items}
              hasSearchOrFilters={Boolean(catalog.query) || catalog.hasActiveFilters}
              isLoading={catalog.isLoading}
              isUsingSeededProducts={catalog.isUsingSeededProducts}
              error={catalog.error}
              onRetry={catalog.retry}
              onClearSearchAndFilters={catalog.clearSearchAndFilters}
              renderProductActions={(product) => <BasketProductControls product={product} />}
            />
            {!catalog.isLoading && !catalog.isUsingSeededProducts && !catalog.error && (
              <CatalogPagination
                page={catalog.page}
                pageCount={catalog.pageCount}
                onPageChange={handlePageChange}
              />
            )}
          </div>
        </section>
      </div>
    </main>
  );
}
