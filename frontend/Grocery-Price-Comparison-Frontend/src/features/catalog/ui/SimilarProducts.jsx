import { useEffect, useState } from "react";
import { getProductsByIds, searchItems } from "../api/catalogApi";
import ProductCard from "./ProductCard";

export default function SimilarProducts({ product }) {
  const [state, setState] = useState({ loading: true, items: [], error: false });

  useEffect(() => {
    const controller = new AbortController();

    async function load() {
      try {
        const results = await searchItems(product.name, undefined, controller.signal);
        const ids = [...new Set(results.map((item) => String(item.id)))]
          .filter((id) => id !== String(product.id))
          .slice(0, 10);
        const products = await getProductsByIds(ids, controller.signal);
        const productsById = new Map(products.map((item) => [String(item.id), item]));
        if (!controller.signal.aborted) {
          setState({ loading: false, items: ids.map((id) => productsById.get(id)).filter(Boolean), error: false });
        }
      } catch {
        if (!controller.signal.aborted) {
          setState({ loading: false, items: [], error: true });
        }
      }
    }

    load();
    return () => controller.abort();
  }, [product.id, product.name]);

  return (
    <section aria-labelledby="similar-products" className="mt-10">
      <h2 id="similar-products" className="text-xl font-semibold">Similar Products</h2>
      {state.loading ? (
        <p className="mt-5 text-sm text-muted-foreground" role="status">Loading similar products...</p>
      ) : state.error ? (
        <p className="mt-5 text-sm text-muted-foreground" role="status">Similar products could not be loaded.</p>
      ) : state.items.length === 0 ? (
        <p className="mt-5 text-sm text-muted-foreground">No similar products found.</p>
      ) : (
        <div className="mt-5 flex gap-4 overflow-x-auto pb-4">
          {state.items.map((item) => (
            <div key={item.id} className="w-64 shrink-0 sm:w-72">
              <ProductCard product={item} />
            </div>
          ))}
        </div>
      )}
    </section>
  );
}
