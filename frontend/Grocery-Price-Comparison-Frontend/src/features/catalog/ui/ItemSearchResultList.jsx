export default function ItemSearchResultList({ items }) {
  if (items.length === 0) {
    return null;
  }

  return (
    <ul className="m-0 list-none p-0">
      {items.map((item) => (
        <li
          key={`${item.id}-${item.store}`}
          className="flex items-baseline justify-between gap-4 border-b border-border px-4 py-3 last:border-b-0"
        >
          <div>
            <p className="m-0 font-medium">{item.name}</p>
            <p className="mt-1 text-sm text-muted-foreground">{item.store}</p>
          </div>
          <p className="m-0 whitespace-nowrap font-medium">{item.cost}</p>
        </li>
      ))}
    </ul>
  );
}
