import { useState } from "react";
import ItemSearchDropdown from "../items/ItemSearchDropdown";
import { Card, CardContent } from "../ui/card";
import { Button } from "../ui/button";
import { Input } from "../ui/input";

export default function Header({ onSearch, searchState }) {
  const [itemName, setItemName] = useState("");
  const { isLoading, items, error, hasSearched } = searchState;

  function handleSubmit(event) {
    event.preventDefault();

    const trimmedItemName = itemName.trim();
    if (trimmedItemName) {
      onSearch(trimmedItemName);
    }
  }

  return (
    <header className="fixed top-4 left-1/2 z-20 w-[min(40rem,calc(100%-2rem))] -translate-x-1/2">
      <Card className="overflow-visible border-border/80 bg-background/95 py-2 shadow-lg backdrop-blur">
        <CardContent className="flex items-center gap-3 px-3">
          <a
            className="hidden shrink-0 text-sm font-semibold tracking-tight sm:block"
            href="/"
          >
            Grocery Price Comparison
          </a>

          <div className="relative min-w-0 flex-1">
            <form className="flex gap-2" onSubmit={handleSubmit} role="search">
              <label className="sr-only" htmlFor="item-search">
                Search for an item
              </label>
              <Input
                id="item-search"
                name="itemName"
                type="search"
                value={itemName}
                onChange={(event) => setItemName(event.target.value)}
                placeholder="Search groceries"
                aria-controls="item-search-results"
                aria-expanded={hasSearched}
                className="h-10 bg-background"
                required
              />
              <Button type="submit" size="lg" disabled={isLoading}>
                {isLoading ? "Searching" : "Search"}
              </Button>
            </form>

            <ItemSearchDropdown
              items={items}
              isLoading={isLoading}
              error={error}
              isOpen={hasSearched}
            />
          </div>
        </CardContent>
      </Card>
    </header>
  );
}
