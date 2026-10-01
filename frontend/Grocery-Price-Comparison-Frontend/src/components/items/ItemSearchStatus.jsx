export default function ItemSearchStatus({ isLoading, error, hasResults }) {
  if (isLoading) {
    return <p className="p-4 text-muted-foreground">Searching…</p>;
  }

  if (error) {
    return (
      <p className="p-4 text-muted-foreground" role="alert">
        {error}
      </p>
    );
  }

  if (!hasResults) {
    return <p className="p-4 text-muted-foreground">No similar items found.</p>;
  }

  return null;
}
