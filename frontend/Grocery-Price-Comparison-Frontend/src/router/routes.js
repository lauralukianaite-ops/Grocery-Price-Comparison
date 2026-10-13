export const APP_ROUTE_PATHS = Object.freeze({
  products: "products",
});

export const APP_ROUTES = Object.freeze({
  home: "/",
  products: `/${APP_ROUTE_PATHS.products}`,
});

export const PRIMARY_NAVIGATION = Object.freeze([
  { label: "Home", to: APP_ROUTES.home, end: true },
  { label: "Products", to: APP_ROUTES.products, end: true },
]);
