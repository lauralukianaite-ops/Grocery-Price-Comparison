import { Route, Routes } from "react-router-dom";
import AppShell from "./components/layout/AppShell";
import HomePage from "./pages/HomePage";
import NotFoundPage from "./pages/NotFoundPage";
import ProductsPage from "./pages/ProductsPage";
import { APP_ROUTE_PATHS } from "./router/routes";

export default function App() {
  return (
    <Routes>
      <Route element={<AppShell />}>
        <Route index element={<HomePage />} />
        <Route path={APP_ROUTE_PATHS.products} element={<ProductsPage />} />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  );
}
