import { Card, CardContent, CardHeader } from "../../../components/ui/card";
import { Skeleton } from "../../../components/ui/skeleton";

export default function ProductGridSkeleton({ count = 8 }) {
  return (
    <div
      className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4"
      aria-label="Loading products"
      aria-busy="true"
    >
      {Array.from({ length: count }, (_, index) => (
        <Card className="gap-0 p-2" key={index}>
          <Skeleton className="aspect-[4/3] rounded-xl" />
          <CardHeader className="gap-2 px-3 pt-4 pb-3">
            <Skeleton className="h-5 w-4/5" />
            <Skeleton className="h-3 w-2/5" />
          </CardHeader>
          <CardContent className="space-y-4 px-3 pb-3">
            <div className="flex justify-between gap-4">
              <Skeleton className="h-8 w-20" />
              <Skeleton className="h-8 w-24" />
            </div>
            <Skeleton className="h-9 w-full" />
          </CardContent>
        </Card>
      ))}
    </div>
  );
}
