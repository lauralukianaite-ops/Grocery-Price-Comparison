import { useMemo, useState } from "react";
import {
  Ban,
  ChevronLeft,
  ChevronRight,
  Heart,
  PencilLine,
  Sparkles,
  Store,
} from "lucide-react";
import { formatPrice } from "../../catalog/productFormatting";
import { Badge } from "../../../components/ui/badge";
import { Button } from "../../../components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
} from "../../../components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "../../../components/ui/dialog";
import BasketRoutePanel from "./BasketRoutePanel";

const alternativeCartPlaceholders = [1, 2, 3];

const initialMockCartItems = [
  { id: "milk", name: "Rimi Milk 2.5%", quantity: 1, unitCost: 1.39 },
  { id: "bread", name: "Rimi Wholegrain Bread", quantity: 1, unitCost: 1.89 },
  { id: "eggs", name: "Rimi Free-range Eggs", quantity: 1, unitCost: 2.79 },
];

const mockSimilarItems = [
  { id: "milk-1", itemId: "milk", name: "Rimi Fresh Milk 2.5%", unitCost: 1.35 },
  { id: "milk-2", itemId: "milk", name: "Rimi Organic Milk 2.5%", unitCost: 2.19 },
  { id: "bread-1", itemId: "bread", name: "Rimi Rye Wholegrain Bread", unitCost: 1.79 },
  { id: "bread-2", itemId: "bread", name: "Rimi Seeded Toast Bread", unitCost: 1.99 },
  { id: "eggs-1", itemId: "eggs", name: "Rimi Barn Eggs, 10 pcs", unitCost: 2.49 },
  { id: "eggs-2", itemId: "eggs", name: "Rimi Organic Eggs, 10 pcs", unitCost: 3.49 },
];

function PendingCartDetails() {
  return (
    <div className="mt-5">
      <div className="grid grid-cols-2 gap-3 border-y py-3">
        <div>
          <p className="text-xs text-muted-foreground">Cart total</p>
          <p className="mt-1 text-lg font-semibold">—</p>
        </div>
        <div>
          <p className="text-xs text-muted-foreground">Shopping time</p>
          <p className="mt-1 text-lg font-semibold">—</p>
        </div>
      </div>
      <div className="mt-4 space-y-3" aria-hidden="true">
        <div className="h-3 w-4/5 rounded-full bg-muted" />
        <div className="h-3 w-3/5 rounded-full bg-muted" />
      </div>
    </div>
  );
}

function CartItem({ item, onChange }) {
  return (
    <li className="flex items-center justify-between gap-3 rounded-lg border bg-background/80 px-3 py-2.5">
      <div className="min-w-0">
        <p className="truncate text-sm font-medium">{item.name}</p>
        <p className="mt-0.5 text-xs text-muted-foreground">
          {item.quantity} item{item.quantity === 1 ? "" : "s"}
        </p>
      </div>
      <div className="flex shrink-0 items-center gap-2">
        <p className="text-sm font-semibold tabular-nums">
          {formatPrice(item.unitCost * item.quantity)}
        </p>
        <Button type="button" variant="outline" size="xs" onClick={onChange}>
          <PencilLine aria-hidden="true" />
          Change item
        </Button>
      </div>
    </li>
  );
}

function SelectedItemCard({ item }) {
  return (
    <article className="rounded-lg border border-primary/40 bg-primary/5 p-3">
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-sm font-medium">{item.name}</p>
          <p className="mt-1 text-xs text-muted-foreground">Rimi</p>
        </div>
        <Badge>Current item</Badge>
      </div>
      <p className="mt-3 text-sm font-semibold tabular-nums">
        {formatPrice(item.unitCost)}
      </p>
    </article>
  );
}

function CandidateCard({ candidate, isBanned, isFavorite, onBan, onFavorite, onUse }) {
  return (
    <article
      className={`rounded-lg border bg-background p-3 transition-opacity ${
        isBanned ? "opacity-55" : ""
      }`}
    >
      <div className="flex items-start justify-between gap-2">
        <div className="min-w-0">
          <p className="line-clamp-2 text-sm font-medium">{candidate.name}</p>
          <p className="mt-1 text-xs text-muted-foreground">Rimi</p>
        </div>
        <div className="flex shrink-0 gap-1">
          <Button
            type="button"
            size="icon-xs"
            variant="ghost"
            aria-label={`${isFavorite ? "Remove" : "Add"} ${candidate.name} ${
              isFavorite ? "from" : "to"
            } favorites`}
            aria-pressed={isFavorite}
            title={isFavorite ? "Remove from favorites" : "Add to favorites"}
            className={isFavorite ? "text-rose-600 hover:text-rose-700" : ""}
            onClick={onFavorite}
          >
            <Heart className={isFavorite ? "fill-current" : ""} aria-hidden="true" />
          </Button>
          <Button
            type="button"
            size="icon-xs"
            variant="ghost"
            aria-label={`${isBanned ? "Allow" : "Ban"} ${candidate.name}`}
            aria-pressed={isBanned}
            title={isBanned ? "Allow this item" : "Ban this item"}
            className={isBanned ? "text-destructive hover:text-destructive" : ""}
            onClick={onBan}
          >
            <Ban aria-hidden="true" />
          </Button>
        </div>
      </div>
      <div className="mt-3 flex items-center justify-between gap-2">
        <p className="text-sm font-semibold tabular-nums">
          {formatPrice(candidate.unitCost)}
        </p>
        <Button
          type="button"
          size="sm"
          disabled={isBanned}
          onClick={onUse}
        >
          Use item
        </Button>
      </div>
    </article>
  );
}

export default function BasketComparisonPreview() {
  const [cartItems, setCartItems] = useState(initialMockCartItems);
  const [isChangeDialogOpen, setIsChangeDialogOpen] = useState(false);
  const [selectedItemId, setSelectedItemId] = useState(initialMockCartItems[0].id);
  const [favoriteIds, setFavoriteIds] = useState(new Set());
  const [bannedIds, setBannedIds] = useState(new Set());
  const total = useMemo(
    () => cartItems.reduce((sum, item) => sum + item.unitCost * item.quantity, 0),
    [cartItems],
  );
  const selectedItem = cartItems.find((item) => item.id === selectedItemId);

  function toggleItem(setItems, itemId) {
    setItems((currentItems) => {
      const nextItems = new Set(currentItems);

      if (nextItems.has(itemId)) {
        nextItems.delete(itemId);
      } else {
        nextItems.add(itemId);
      }

      return nextItems;
    });
  }

  function selectSimilarItem(candidate) {
    setCartItems((currentItems) =>
      currentItems.map((item) =>
        item.id === candidate.itemId
          ? { ...item, name: candidate.name, unitCost: candidate.unitCost }
          : item,
      ),
    );
    setIsChangeDialogOpen(false);
  }

  return (
    <Dialog open={isChangeDialogOpen} onOpenChange={setIsChangeDialogOpen}>
      <section
        className="mt-5 grid items-start gap-5 xl:grid-cols-[minmax(0,1.18fr)_minmax(26rem,0.82fr)] 2xl:grid-cols-[minmax(0,1.28fr)_minmax(30rem,0.72fr)]"
        aria-label="Optimized basket comparison"
      >
        <div className="space-y-5">
        <Card className="border-2 border-primary/35 bg-primary/[0.035] shadow-sm">
          <CardHeader className="flex flex-row items-start justify-between gap-3">
            <div>
              <div className="flex items-center gap-2">
                <Sparkles className="size-4" aria-hidden="true" />
                <h2 className="font-semibold">Optimal combination</h2>
              </div>
              <p className="mt-1 text-sm text-muted-foreground">
                Rimi + Lidl split · optimized for the lowest trip total.
              </p>
            </div>
            <Badge>Recommended</Badge>
          </CardHeader>
          <CardContent>
            <div className="rounded-xl border bg-background/85 p-4">
              <div className="flex items-center justify-between gap-3 border-b pb-3">
                <div>
                  <p className="text-sm font-medium">Rimi · stop 1</p>
                  <p className="mt-0.5 text-xs text-muted-foreground">
                    {cartItems.length} items
                  </p>
                </div>
                <div className="text-right"><p className="text-lg font-semibold tabular-nums">{formatPrice(total)}</p><p className="text-xs text-emerald-700">Saves €1.85</p></div>
              </div>
              <ul className="mt-3 space-y-2">
                {cartItems.map((item) => (
                  <CartItem
                    key={item.id}
                    item={item}
                    onChange={() => {
                      setSelectedItemId(item.id);
                      setIsChangeDialogOpen(true);
                    }}
                  />
                ))}
              </ul>
            </div>
          </CardContent>
        </Card>

        <Card className="min-w-0 bg-muted/15">
          <CardHeader className="flex flex-row items-start justify-between gap-3">
            <div>
              <div className="flex items-center gap-2">
                <Store className="size-4" aria-hidden="true" />
                <h2 className="font-semibold">Single-store alternatives</h2>
              </div>
              <p className="mt-1 text-sm text-muted-foreground">
                Keep every item in one store instead of splitting the route.
              </p>
            </div>
            <div className="flex gap-1" aria-label="Alternative carts carousel controls">
              <Button size="icon-sm" variant="outline" disabled aria-label="Previous store cart">
                <ChevronLeft aria-hidden="true" />
              </Button>
              <Button size="icon-sm" variant="outline" disabled aria-label="Next store cart">
                <ChevronRight aria-hidden="true" />
              </Button>
            </div>
          </CardHeader>
          <CardContent>
            <div className="grid gap-3 sm:grid-cols-3">
              {alternativeCartPlaceholders.map((placeholder) => (
                <article
                  key={placeholder}
                  className="min-w-0 rounded-xl border bg-background p-4 shadow-sm"
                >
                  <div className="flex items-center justify-between gap-2"><p className="font-medium">Store alternative</p><Badge variant="outline">{placeholder === 1 ? "Nearest" : "Option"}</Badge></div>
                  <PendingCartDetails />
                </article>
              ))}
            </div>
          </CardContent>
        </Card>
        </div>
        <BasketRoutePanel />
      </section>

      <DialogContent className="max-h-[calc(100dvh-2rem)] max-w-4xl overflow-y-auto p-6 sm:max-w-4xl">
        <DialogHeader>
          <DialogTitle>Change item</DialogTitle>
          <DialogDescription>
            The current item is shown first. Other name-similar Rimi items are ordered by price.
          </DialogDescription>
        </DialogHeader>

        {selectedItem && (
          <section aria-labelledby="current-item">
            <div className="mb-2 flex items-center justify-between gap-3">
              <h3 id="current-item" className="text-sm font-medium">Current selection</h3>
              <span className="text-xs text-muted-foreground">Rimi</span>
            </div>
            <SelectedItemCard item={selectedItem} />

            <div className="mb-2 mt-5 flex items-center justify-between gap-3">
              <h3 className="text-sm font-medium">Similar items</h3>
              <span className="text-xs text-muted-foreground">Lowest price first</span>
            </div>
            <div className="grid gap-3 sm:grid-cols-2">
              {mockSimilarItems
                .filter(
                  (candidate) =>
                    candidate.itemId === selectedItem.id &&
                    candidate.name !== selectedItem.name,
                )
                .sort((left, right) => left.unitCost - right.unitCost)
                .map((candidate) => (
                  <CandidateCard
                    key={candidate.id}
                    candidate={candidate}
                    isBanned={bannedIds.has(candidate.id)}
                    isFavorite={favoriteIds.has(candidate.id)}
                    onBan={() => toggleItem(setBannedIds, candidate.id)}
                    onFavorite={() => toggleItem(setFavoriteIds, candidate.id)}
                    onUse={() => selectSimilarItem(candidate)}
                  />
                ))}
            </div>
          </section>
        )}
      </DialogContent>
    </Dialog>
  );
}

