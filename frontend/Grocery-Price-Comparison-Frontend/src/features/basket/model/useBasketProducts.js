import { useCallback, useEffect, useMemo, useState } from "react";
import { getProductsByIds } from "../../catalog/api/catalogApi";
import { getSeededProduct } from "../../catalog/seededProducts";

function resolveSeededProducts(productIds) {
  return productIds.map(getSeededProduct).filter(Boolean);
}

export function useBasketProducts(items) {
  const productIds = useMemo(
    () => items.map((item) => item.productId),
    [items],
  );
  const [requestVersion, setRequestVersion] = useState(0);
  const seededProducts = useMemo(
    () => resolveSeededProducts(productIds),
    [productIds],
  );
  const unresolvedIdsKey = useMemo(() => {
    const seededIds = new Set(
      seededProducts.map((product) => String(product.id)),
    );

    return productIds
      .filter((productId) => !seededIds.has(productId))
      .join("|");
  }, [productIds, seededProducts]);
  const requestKey = `${unresolvedIdsKey}:${requestVersion}`;
  const [requestState, setRequestState] = useState({
    key: "",
    products: [],
    error: "",
  });

  const retry = useCallback(() => {
    setRequestVersion((current) => current + 1);
  }, []);

  useEffect(() => {
    if (!unresolvedIdsKey) {
      return undefined;
    }

    const controller = new AbortController();

    getProductsByIds(unresolvedIdsKey.split("|"), controller.signal)
      .then((products) => {
        setRequestState({ key: requestKey, products, error: "" });
      })
      .catch((requestError) => {
        if (requestError.name !== "AbortError") {
          setRequestState({
            key: requestKey,
            products: [],
            error: requestError.message,
          });
        }
      });

    return () => controller.abort();
  }, [requestKey, unresolvedIdsKey]);

  const hasCurrentResponse = requestState.key === requestKey;
  const error = hasCurrentResponse ? requestState.error : "";
  const isLoading = Boolean(unresolvedIdsKey) && !hasCurrentResponse;

  const products = useMemo(
    () => [
      ...seededProducts,
      ...(hasCurrentResponse ? requestState.products : []),
    ],
    [hasCurrentResponse, requestState.products, seededProducts],
  );
  const productsById = useMemo(
    () => new Map(products.map((product) => [String(product.id), product])),
    [products],
  );
  const lines = items.flatMap((item) => {
    const product = productsById.get(item.productId);
    return product ? [{ ...item, product }] : [];
  });

  return {
    lines,
    unavailableProductCount: items.length - lines.length,
    isLoading,
    error,
    retry,
  };
}
