export const APP_ROUTE_PATHS = Object.freeze({
  products: "products",
  productDetail: "products/:productId",
  basket: "basket",
});

export const APP_ROUTES = Object.freeze({
  home: "/",
  products: `/${APP_ROUTE_PATHS.products}`,
  basket: `/${APP_ROUTE_PATHS.basket}`,
});

export function getProductRoute(productId) {
  return `${APP_ROUTES.products}/${encodeURIComponent(String(productId))}`;
}

export const PRIMARY_NAVIGATION = Object.freeze([
  { label: "Home", to: APP_ROUTES.home, end: true },
  { label: "Products", to: APP_ROUTES.products, end: true },
]);
