import { Clock3, MapPin, Scale, WalletCards } from "lucide-react";
import { useEffect } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { Button, buttonVariants } from "../../../components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
} from "../../../components/ui/card";
import { Badge } from "../../../components/ui/badge";
import { useLocationStore } from "../../location";
import {
  BASKET_STRATEGIES,
  DEFAULT_BASKET_STRATEGY,
  isBasketStrategy,
  parseBasketStrategy,
} from "../model/basketStrategies";
import { cn } from "../../../lib/utils";
import { APP_ROUTES } from "../../../router/routes";

const STRATEGY_ICONS = {
  fastest: Clock3,
  balanced: Scale,
  cheapest: WalletCards,
};

export default function BasketStrategySelector() {
  const [searchParams, setSearchParams] = useSearchParams();
  const currentLocation = useLocationStore((state) => state.currentLocation);
  const rawStrategy = searchParams.get("strategy");
  const selectedStrategy = parseBasketStrategy(rawStrategy);
  const selectedOption = BASKET_STRATEGIES.find(
    (strategy) => strategy.value === selectedStrategy,
  );

  useEffect(() => {
    if (rawStrategy !== null && !isBasketStrategy(rawStrategy)) {
      setSearchParams(
        (currentSearchParams) => {
          const nextSearchParams = new URLSearchParams(currentSearchParams);
          nextSearchParams.delete("strategy");
          return nextSearchParams;
        },
        { replace: true },
      );
    }
  }, [rawStrategy, setSearchParams]);

  function selectStrategy(strategy) {
    setSearchParams((currentSearchParams) => {
      const nextSearchParams = new URLSearchParams(currentSearchParams);

      if (strategy === DEFAULT_BASKET_STRATEGY) {
        nextSearchParams.delete("strategy");
      } else {
        nextSearchParams.set("strategy", strategy);
      }

      return nextSearchParams;
    });
  }

  return (
    <Card className="mt-7">
      <CardHeader className="flex flex-row flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-base font-semibold">Choose your shopping strategy</h2>
          <p className="mt-1 text-sm text-muted-foreground">
            Select how the future route recommendation should prioritize your trip.
          </p>
        </div>
        <Badge variant="outline">Route optimization coming soon</Badge>
      </CardHeader>
      <CardContent>
        <div
          className="grid grid-cols-1 gap-2 sm:grid-cols-3"
          role="group"
          aria-label="Shopping route strategy"
        >
          {BASKET_STRATEGIES.map((strategy) => {
            const Icon = STRATEGY_ICONS[strategy.value];
            const isSelected = strategy.value === selectedStrategy;

            return (
              <Button
                key={strategy.value}
                type="button"
                size="lg"
                variant={isSelected ? "default" : "outline"}
                aria-pressed={isSelected}
                onClick={() => selectStrategy(strategy.value)}
              >
                <Icon aria-hidden="true" />
                {strategy.label}
              </Button>
            );
          })}
        </div>

        <div className="mt-4 flex flex-col gap-3 rounded-lg bg-muted/50 p-3 text-sm sm:flex-row sm:items-center sm:justify-between">
          <p aria-live="polite">
            <span className="font-medium">{selectedOption.label}:</span>{" "}
            <span className="text-muted-foreground">
              {selectedOption.description}
            </span>
          </p>
          {currentLocation ? (
            <span className="flex shrink-0 items-center gap-1.5 text-xs font-medium text-emerald-700">
              <MapPin className="size-3.5" aria-hidden="true" />
              Saved location ready
            </span>
          ) : (
            <Link
              className={cn(
                buttonVariants({ variant: "ghost", size: "sm" }),
                "shrink-0",
              )}
              to={APP_ROUTES.products}
            >
              <MapPin aria-hidden="true" />
              Add a location
            </Link>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
