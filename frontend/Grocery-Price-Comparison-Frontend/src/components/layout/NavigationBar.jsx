import { useState } from "react";
import { Menu, ShoppingBasket } from "lucide-react";
import { Link, NavLink } from "react-router-dom";
import {
  selectBasketItemCount,
  useBasketStore,
} from "../../features/basket";
import { APP_ROUTES, PRIMARY_NAVIGATION } from "../../router/routes";
import { cn } from "../../lib/utils";
import { Badge } from "../ui/badge";
import { Button, buttonVariants } from "../ui/button";
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "../ui/sheet";

function navigationLinkClass({ isActive }) {
  return cn(
    buttonVariants({ variant: isActive ? "secondary" : "ghost" }),
    "justify-start",
  );
}

export default function NavigationBar() {
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const basketCount = useBasketStore(selectBasketItemCount);

  return (
    <>
      <a
        href="#main-content"
        className="fixed top-2 left-2 z-[60] -translate-y-20 rounded-md bg-primary px-3 py-2 text-sm font-medium text-primary-foreground shadow focus:translate-y-0"
      >
        Skip to content
      </a>

      <header className="sticky top-0 z-40 border-b border-border/80 bg-background/95 backdrop-blur">
        <div className="mx-auto flex h-16 w-full max-w-[var(--page-content-width)] items-center gap-3 px-4 sm:px-6 lg:px-8 xl:px-10">
          <Link
            className="flex shrink-0 items-center gap-2 rounded-md outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
            to={APP_ROUTES.home}
            aria-label="Pricebasket home"
          >
            <span className="grid size-8 place-items-center rounded-lg bg-primary text-primary-foreground">
              <ShoppingBasket className="size-4" aria-hidden="true" />
            </span>
            <span className="hidden font-semibold tracking-tight sm:block">
              Pricebasket
            </span>
          </Link>

          <nav
            className="hidden items-center gap-1 md:flex"
            aria-label="Primary navigation"
          >
            {PRIMARY_NAVIGATION.map((item) => (
              <NavLink
                className={navigationLinkClass}
                end={item.end}
                key={item.to}
                to={item.to}
              >
                {item.label}
              </NavLink>
            ))}
          </nav>

          <div className="ml-auto flex items-center gap-2">
            <Link
              className={cn(buttonVariants({ variant: "outline" }), "relative")}
              to={APP_ROUTES.basket}
              aria-label={`Basket with ${basketCount} ${basketCount === 1 ? "item" : "items"}`}
            >
              <ShoppingBasket aria-hidden="true" />
              <span className="hidden sm:inline">Basket</span>
              {basketCount > 0 && (
                <Badge className="absolute -top-2 -right-2 size-5 px-0 text-[0.65rem]">
                  {basketCount}
                </Badge>
              )}
            </Link>

            <Sheet open={isMobileMenuOpen} onOpenChange={setIsMobileMenuOpen}>
              <SheetTrigger
                render={
                  <Button
                    type="button"
                    variant="outline"
                    size="icon"
                    className="md:hidden"
                    aria-label="Open navigation menu"
                  />
                }
              >
                <Menu aria-hidden="true" />
              </SheetTrigger>
              <SheetContent side="right" className="w-[18rem] p-4 sm:max-w-sm">
                <SheetHeader className="px-0 pt-2">
                  <SheetTitle>Navigation</SheetTitle>
                  <SheetDescription>Browse Pricebasket.</SheetDescription>
                </SheetHeader>
                <nav
                  className="flex flex-col gap-1 pt-2"
                  aria-label="Mobile navigation"
                >
                  {PRIMARY_NAVIGATION.map((item) => (
                    <NavLink
                      className={navigationLinkClass}
                      end={item.end}
                      key={item.to}
                      onClick={() => setIsMobileMenuOpen(false)}
                      to={item.to}
                    >
                      {item.label}
                    </NavLink>
                  ))}
                </nav>
              </SheetContent>
            </Sheet>
          </div>
        </div>
      </header>
    </>
  );
}
