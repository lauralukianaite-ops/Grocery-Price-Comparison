import { ShoppingBasket } from "lucide-react";
import { Link } from "react-router-dom";
import { APP_ROUTES } from "../../router/routes";

const FOOTER_LINKS = [
  { label: "Home", to: APP_ROUTES.home },
  { label: "Products", to: APP_ROUTES.products },
  { label: "Basket", to: APP_ROUTES.basket },
];

export default function WebsiteFooter() {
  return (
    <footer className="border-t border-border bg-muted/30">
      <div className="mx-auto w-full max-w-[var(--page-content-width)] px-4 py-10 sm:px-6 lg:px-8 xl:px-10">
        <div className="flex flex-col gap-7 sm:flex-row sm:items-start sm:justify-between">
          <div className="max-w-md">
            <Link
              className="inline-flex items-center gap-2 rounded-md font-semibold outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
              to={APP_ROUTES.home}
              aria-label="Pricebasket home"
            >
              <span className="grid size-8 place-items-center rounded-lg bg-primary text-primary-foreground">
                <ShoppingBasket className="size-4" aria-hidden="true" />
              </span>
              Pricebasket
            </Link>
            <p className="mt-3 text-sm leading-6 text-muted-foreground">
              Compare grocery prices across participating stores and keep your
              preferred products together.
            </p>
          </div>

          <nav aria-label="Footer navigation">
            <p className="text-sm font-semibold">Explore</p>
            <ul className="mt-3 flex flex-wrap gap-x-5 gap-y-2 text-sm text-muted-foreground">
              {FOOTER_LINKS.map((link) => (
                <li key={link.to}>
                  <Link className="hover:text-foreground hover:underline" to={link.to}>
                    {link.label}
                  </Link>
                </li>
              ))}
            </ul>
          </nav>
        </div>

        <div className="mt-8 flex flex-col gap-2 border-t border-border pt-5 text-xs text-muted-foreground sm:flex-row sm:justify-between">
          <p>© {new Date().getFullYear()} Pricebasket.</p>
          <p>Prices and availability may change at participating stores.</p>
        </div>
      </div>
    </footer>
  );
}
