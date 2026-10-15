import { LocateFixed, MapPin, X } from "lucide-react";
import { Button } from "../../../components/ui/button";
import { Card, CardContent } from "../../../components/ui/card";
import { useCurrentLocation } from "../model/useCurrentLocation";

function formatCoordinate(value, positiveDirection, negativeDirection) {
  const direction = value >= 0 ? positiveDirection : negativeDirection;
  return `${Math.abs(value).toFixed(3)}° ${direction}`;
}

export default function LocationPreference() {
  const {
    currentLocation,
    error,
    isRequesting,
    requestCurrentLocation,
    clearCurrentLocation,
  } = useCurrentLocation();

  return (
    <Card className="mt-6 bg-muted/35 py-0">
      <CardContent className="flex flex-col gap-4 p-4 sm:flex-row sm:items-center">
        <span className="grid size-10 shrink-0 place-items-center rounded-lg bg-background text-foreground ring-1 ring-border">
          <MapPin className="size-5" aria-hidden="true" />
        </span>

        <div className="min-w-0 flex-1">
          <h2 className="text-sm font-semibold">
            {currentLocation
              ? "Using your saved location"
              : "Find better prices near you"}
          </h2>
          <p className="mt-1 text-sm leading-5 text-muted-foreground">
            {currentLocation
              ? `${formatCoordinate(currentLocation.latitude, "N", "S")}, ${formatCoordinate(currentLocation.longitude, "E", "W")} · Saved in this browser for future visits.`
              : "Share your current location to request nearby prices. It is saved in this browser and sent only with product-price requests."}
          </p>
          {error && (
            <p className="mt-2 text-sm text-destructive" role="alert">
              {error}
            </p>
          )}
        </div>

        <div className="flex shrink-0 gap-2">
          <Button
            type="button"
            variant={currentLocation ? "outline" : "default"}
            onClick={requestCurrentLocation}
            disabled={isRequesting}
          >
            <LocateFixed aria-hidden="true" />
            {isRequesting
              ? "Finding location…"
              : currentLocation
                ? "Update"
                : "Use my location"}
          </Button>
          {currentLocation && (
            <Button
              type="button"
              variant="ghost"
              size="icon"
              onClick={clearCurrentLocation}
              aria-label="Remove saved location"
            >
              <X aria-hidden="true" />
            </Button>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
