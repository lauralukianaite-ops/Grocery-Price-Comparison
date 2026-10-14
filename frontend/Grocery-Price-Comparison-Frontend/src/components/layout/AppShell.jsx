import { useEffect } from "react";
import { Outlet, useLocation } from "react-router-dom";
import { APP_ROUTES } from "../../router/routes";
import NavigationBar from "./NavigationBar";
import WebsiteFooter from "./WebsiteFooter";

const PAGE_TITLES = {
  [APP_ROUTES.home]: "Pricebasket — Grocery price comparison",
  [APP_ROUTES.products]: "All products — Pricebasket",
  [APP_ROUTES.basket]: "Your basket — Pricebasket",
};

function getPageTitle(pathname) {
  if (pathname.startsWith(`${APP_ROUTES.products}/`)) {
    return "Product details — Pricebasket";
  }

  return PAGE_TITLES[pathname] ?? "Page not found — Pricebasket";
}

export default function AppShell() {
  const { pathname } = useLocation();

  useEffect(() => {
    document.title = getPageTitle(pathname);
    window.scrollTo({ top: 0, left: 0, behavior: "auto" });
  }, [pathname]);

  return (
    <div className="flex min-h-screen flex-col bg-background">
      <NavigationBar />
      <div className="flex-1">
        <Outlet />
      </div>
      <WebsiteFooter />
    </div>
  );
}
