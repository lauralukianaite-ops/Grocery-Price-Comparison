import { Skeleton } from "../ui/skeleton";

export default function PageLoadingFallback() {
  return (
    <main
      id="main-content"
      aria-busy="true"
      aria-label="Loading page"
      className="mx-auto w-full max-w-[var(--page-content-width)] px-4 py-10 sm:px-6 lg:px-8 xl:px-10"
    >
      <Skeleton className="h-8 w-32" />
      <div className="mt-8 grid gap-8 lg:grid-cols-2">
        <Skeleton className="aspect-[4/3] rounded-xl" />
        <div className="space-y-4">
          <Skeleton className="h-5 w-24" />
          <Skeleton className="h-10 w-3/4" />
          <Skeleton className="h-32 w-full rounded-xl" />
        </div>
      </div>
    </main>
  );
}
