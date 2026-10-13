import { ImageIcon } from "lucide-react";
import { Skeleton } from "../ui/skeleton";

export default function ProductVisual({ product }) {
  if (product.imageUrl) {
    return (
      <div className="aspect-[4/3] overflow-hidden rounded-xl bg-muted">
        <img
          className="size-full object-contain p-4"
          src={product.imageUrl}
          alt=""
          loading="lazy"
        />
      </div>
    );
  }

  return (
    <Skeleton
      className="grid aspect-[4/3] place-items-center rounded-xl"
      aria-label={`Image for ${product.name} is not available yet`}
    >
      <ImageIcon className="size-9 text-muted-foreground/50" aria-hidden="true" />
    </Skeleton>
  );
}
