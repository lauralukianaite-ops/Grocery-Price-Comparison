import { BasketProductControls } from "../features/basket";
import {
  CatalogHeader,
  CatalogPagination,
  CatalogResults,
  CatalogToolbar,
  useCatalogPage,
} from "../features/catalog";
import { LocationPreference } from "../features/location";

export default function ProductsPage() {
  const catalog = useCatalogPage();

  return (
    <main
      id="main-content"
      tabIndex="-1"
      className="mx-auto w-full max-w-[var(--page-content-width)] px-4 py-10 sm:px-6 sm:py-14 lg:px-8 xl:px-10"
    >
      <CatalogHeader
        isLoading={catalog.isLoading}
        isUsingSeededProducts={catalog.isUsingSeededProducts}
        totalCount={catalog.totalCount}
      />

      <LocationPreference />

      <section id="products" className="mt-8" aria-label="Product catalog">
        <CatalogToolbar
          query={catalog.query}
          sort={catalog.sort}
          isLoading={catalog.isLoading}
          isUsingSeededProducts={catalog.isUsingSeededProducts}
          onQueryChange={catalog.updateQuery}
          onSortChange={catalog.updateSort}
        />

        <CatalogResults
          items={catalog.items}
          query={catalog.query}
          isLoading={catalog.isLoading}
          isUsingSeededProducts={catalog.isUsingSeededProducts}
          onRetry={catalog.retry}
          onClearSearch={() => catalog.updateQuery("")}
          renderProductActions={(product) => (
            <BasketProductControls product={product} />
          )}
        />

        {!catalog.isLoading && !catalog.isUsingSeededProducts && (
          <CatalogPagination
            page={catalog.page}
            pageCount={catalog.pageCount}
            onPageChange={catalog.changePage}
          />
        )}
      </section>
    </main>
  );
}
