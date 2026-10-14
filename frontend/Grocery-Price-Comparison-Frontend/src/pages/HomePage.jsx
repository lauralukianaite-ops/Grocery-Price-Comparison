import Hero from "../components/home/Hero";
import HomeSearch from "../components/home/HomeSearch";
import { useItemSearch } from "../features/catalog";

export default function HomePage() {
  const { search, items, isLoading, error, hasSearched } = useItemSearch();

  return (
    <main id="main-content" tabIndex="-1">
      <Hero>
        <HomeSearch
          onSearch={search}
          searchState={{ items, isLoading, error, hasSearched }}
        />
      </Hero>
    </main>
  );
}
