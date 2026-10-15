import { ArrowRight, ShoppingBasket } from "lucide-react";
import { Link } from "react-router-dom";
import { buttonVariants } from "../../../components/ui/button";
import { Card, CardContent } from "../../../components/ui/card";
import { cn } from "../../../lib/utils";
import { APP_ROUTES } from "../../../router/routes";

export default function BasketEmptyState() {
  return (
    <main
      id="main-content"
      tabIndex="-1"
      className="mx-auto max-w-3xl px-4 py-16 sm:px-6 lg:px-8"
    >
      <Card className="border-dashed py-14 text-center">
        <CardContent>
          <ShoppingBasket className="mx-auto size-10 text-muted-foreground" />
          <h1 className="mt-4 text-2xl font-semibold">Your basket is empty</h1>
          <p className="mt-2 text-sm text-muted-foreground">
            Add products to keep their best available prices together.
          </p>
          <Link
            className={cn(buttonVariants(), "mt-6")}
            to={APP_ROUTES.products}
          >
            Browse products
            <ArrowRight aria-hidden="true" />
          </Link>
        </CardContent>
      </Card>
    </main>
  );
}
