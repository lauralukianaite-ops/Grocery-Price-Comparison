import { lazy, Suspense } from "react";
import { Route, Routes } from "react-router-dom";
import AppShell from "./components/layout/AppShell";
import PageLoadingFallback from "./components/layout/PageLoadingFallback";
import HomePage from "./pages/HomePage";
import BasketPage from "./pages/BasketPage";
import NotFoundPage from "./pages/NotFoundPage";
import ProductsPage from "./pages/ProductsPage";
import { APP_ROUTE_PATHS } from "./router/routes";

const ProductDetailPage = lazy(() => import("./pages/ProductDetailPage"));

export default function App() {
  return (
    <Routes>
      <Route element={<AppShell />}>
        <Route index element={<HomePage />} />
        <Route path={APP_ROUTE_PATHS.products} element={<ProductsPage />} />
        <Route
          path={APP_ROUTE_PATHS.productDetail}
          element={
            <Suspense fallback={<PageLoadingFallback />}>
              <ProductDetailPage />
            </Suspense>
          }
        />
        <Route path={APP_ROUTE_PATHS.basket} element={<BasketPage />} />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  );
}
