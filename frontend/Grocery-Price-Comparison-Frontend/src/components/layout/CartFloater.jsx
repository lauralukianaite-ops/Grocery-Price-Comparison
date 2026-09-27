import { ShoppingCart } from "lucide-react";
import { Button } from "../ui/button";
import { Card, CardContent } from "../ui/card";

export default function CartFloater() {
  return (
    <aside className="fixed top-4 right-4 z-30 max-sm:top-20" aria-label="Cart">
      <Card className="border-border/80 bg-background/95 py-2 shadow-lg backdrop-blur">
        <CardContent className="px-2">
          <Button type="button" variant="outline" size="lg">
            <ShoppingCart aria-hidden="true" />
            Cart
          </Button>
        </CardContent>
      </Card>
    </aside>
  );
}
