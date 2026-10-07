import { Card, CardContent } from "../ui/card";
import ItemSearchResultList from "./ItemSearchResultList";
import ItemSearchStatus from "./ItemSearchStatus";

export default function ItemSearchDropdown({
  items,
  isLoading,
  error,
  isOpen,
}) {
  if (!isOpen) {
    return null;
  }

  return (
    <Card
      id="item-search-results"
      className="absolute top-full right-0 left-0 mt-2 max-h-[min(22rem,45vh)] overflow-y-auto py-0 shadow-lg"
      aria-label="Item search results"
      aria-live="polite"
    >
      <CardContent className="px-0">
        <ItemSearchStatus
          isLoading={isLoading}
          error={error}
          hasResults={items.length > 0}
        />
        <ItemSearchResultList items={items} />
      </CardContent>
    </Card>
  );
}
