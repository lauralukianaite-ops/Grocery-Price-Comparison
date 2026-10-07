import Header from "./components/layout/Header";
import CartFloater from "./components/layout/CartFloater";
import Hero from "./components/home/Hero";
import { useItemSearch } from "./hooks/useItemSearch";

function App() {
  const { search, items, isLoading, error, hasSearched } = useItemSearch();

  return (
    <div className="relative min-h-screen">
      <main>
        <Hero />
      </main>
      <Header
        onSearch={search}
        searchState={{
          items,
          isLoading,
          error,
          hasSearched,
        }}
      />
      <CartFloater />
    </div>
  );
}

export default App;
