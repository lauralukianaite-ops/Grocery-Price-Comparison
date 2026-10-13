import { useState } from "react";
import ItemSearchDropdown from "../items/ItemSearchDropdown";
import { Button } from "../ui/button";
import { Card, CardContent } from "../ui/card";
import { Input } from "../ui/input";

export default function HomeSearch({ onSearch, searchState }) {
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
    <Card className="relative mt-10 overflow-visible border-border/80 bg-background/95 py-2 text-left shadow-lg backdrop-blur">
      <CardContent className="px-3">
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
      </CardContent>
    </Card>
  );
}
