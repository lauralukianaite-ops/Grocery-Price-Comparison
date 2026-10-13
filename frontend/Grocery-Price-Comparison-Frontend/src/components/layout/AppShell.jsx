import { useEffect } from "react";
import { Outlet, useLocation } from "react-router-dom";
import { APP_ROUTES } from "../../router/routes";
import NavigationBar from "./NavigationBar";

const PAGE_TITLES = {
  [APP_ROUTES.home]: "Pricebasket — Grocery price comparison",
  [APP_ROUTES.products]: "All products — Pricebasket",
};

export default function AppShell() {
  const { pathname } = useLocation();

  useEffect(() => {
    document.title = PAGE_TITLES[pathname] ?? "Page not found — Pricebasket";
    window.scrollTo({ top: 0, left: 0, behavior: "auto" });
  }, [pathname]);

  return (
    <div className="min-h-screen bg-background">
      <NavigationBar />
      <Outlet />
    </div>
  );
}
