import { Trash2 } from "lucide-react";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "../../../components/ui/alert-dialog";
import { Button } from "../../../components/ui/button";

export default function BasketHeader({
  itemCount,
  productCount,
  onClearBasket,
}) {
  return (
    <div className="flex flex-wrap items-end justify-between gap-4 border-b pb-7">
      <div>
        <h1 className="text-3xl font-semibold tracking-[-0.035em] sm:text-4xl">
          Your basket
        </h1>
        <p className="mt-2 text-sm text-muted-foreground">
          {`${itemCount} ${itemCount === 1 ? "item" : "items"} across ${productCount} ${productCount === 1 ? "product" : "products"}, saved on this device.`}
        </p>
      </div>

      <AlertDialog>
        <AlertDialogTrigger render={<Button variant="outline" size="sm" />}>
          <Trash2 aria-hidden="true" />
          Clear basket
        </AlertDialogTrigger>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Clear your basket?</AlertDialogTitle>
            <AlertDialogDescription>
              This removes every saved product from this device.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction variant="destructive" onClick={onClearBasket}>
              Clear basket
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
