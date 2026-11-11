import { useCallback, useRef, useState } from "react";
import { Bike, Car, Footprints, MapPin, Navigation, Route, Store } from "lucide-react";
import { useLocationStore } from "../../location";
<<<<<<< HEAD
import { getStartingPointQuery } from "../../location/api/googleMaps";
import { useGoogleMaps } from "../../location/model/useGoogleMaps";
import StartingPointInput from "../../location/ui/StartingPointInput";
import StartingPointMap from "../../location/ui/StartingPointMap";
=======
>>>>>>> integration
import { Button } from "../../../components/ui/button";
import { Badge } from "../../../components/ui/badge";
import { Card, CardContent, CardHeader } from "../../../components/ui/card";
import { ToggleGroup, ToggleGroupItem } from "../../../components/ui/toggle-group";

const transportModes = [
  { value: "driving", label: "Car", icon: Car },
  { value: "bicycling", label: "Bike", icon: Bike },
  { value: "walking", label: "Walk", icon: Footprints },
];

<<<<<<< HEAD
=======
// The comparison currently shows example carts rather than calculated store locations.
>>>>>>> integration
const exampleStops = [
  { name: "Rimi Hyper", query: "Rimi Hyper, Vilnius, Lithuania" },
  { name: "Lidl Žirmūnai", query: "Lidl, Žirmūnai, Vilnius, Lithuania" },
];

export default function BasketRoutePanel() {
  const currentLocation = useLocationStore((state) => state.currentLocation);
<<<<<<< HEAD
  const { libraries, status: mapsStatus, error: mapsError, reportError } = useGoogleMaps();
  const [transport, setTransport] = useState("driving");
  const [startingPoint, setStartingPoint] = useState("");
  const [selectedPoint, setSelectedPoint] = useState(null);
  const [mapAddress, setMapAddress] = useState("");
  const [picking, setPicking] = useState(false);
  const [findingAddress, setFindingAddress] = useState(false);
  const [addressError, setAddressError] = useState("");
  const [inputReset, setInputReset] = useState(0);
  const requestVersion = useRef(0);
  const originPoint = selectedPoint || (startingPoint.trim() ? { address: startingPoint } : currentLocation);
  const origin = getStartingPointQuery(originPoint);
  const mapQuery = mapAddress || getStartingPointQuery(selectedPoint || currentLocation) || "grocery stores in Vilnius, Lithuania";
  const routeParams = new URLSearchParams({
    api: "1",
    destination: exampleStops[1].query,
    waypoints: exampleStops[0].query,
    travelmode: transport,
  });
  if (origin) routeParams.set("origin", origin);

  const selectStartingPoint = useCallback((point) => {
    requestVersion.current++;
    setSelectedPoint(point);
    setStartingPoint(point.address);
    setMapAddress("");
    setPicking(false);
    setFindingAddress(false);
    setAddressError("");
  }, []);

  function changeAddress(value) {
    requestVersion.current++;
    setStartingPoint(value);
    setSelectedPoint(null);
    setPicking(false);
    setFindingAddress(false);
    setAddressError("");
  }

  function clearStartingPoint() {
    changeAddress("");
    setMapAddress("");
    setInputReset((value) => value + 1);
  }

  async function showStartingAddress(event) {
    event.preventDefault();
    const address = startingPoint.trim();
    if (!address) return;
    if (!libraries) {
      setMapAddress(address);
      return;
    }
    const version = ++requestVersion.current;
    setInputReset((value) => value + 1);
    setPicking(false);
    setFindingAddress(true);
    setAddressError("");
    try {
      const { results } = await new libraries.Geocoder().geocode({ address, region: "lt" });
      if (version !== requestVersion.current) return;
      if (!results[0]?.geometry?.location) throw new Error("Address not found");
      const { formatted_address: label, geometry: { location } } = results[0];
      selectStartingPoint({ address: label || address, latitude: location.lat(), longitude: location.lng() });
    } catch {
      if (version === requestVersion.current) setAddressError("Address not found.");
    } finally {
      if (version === requestVersion.current) setFindingAddress(false);
    }
=======
  const [transport, setTransport] = useState("driving");
  const [startingPoint, setStartingPoint] = useState("");
  const [mapAddress, setMapAddress] = useState("");
  const savedOrigin = currentLocation
    ? `${currentLocation.latitude},${currentLocation.longitude}`
    : "";
  const origin = startingPoint.trim() || savedOrigin;
  const mapQuery = mapAddress || savedOrigin || "grocery stores in Vilnius, Lithuania";
  const mapParams = new URLSearchParams({ q: mapQuery, output: "embed" });
  const routeParams = new URLSearchParams({
    api: "1",
    destination: exampleStops[1].query,
    travelmode: transport,
  });

  if (origin) {
    routeParams.set("origin", origin);
  }
  routeParams.set("waypoints", exampleStops[0].query);

  function clearStartingPoint() {
    setStartingPoint("");
    setMapAddress("");
>>>>>>> integration
  }

  return (
    <Card className="h-fit border-primary/15 shadow-sm xl:sticky xl:top-5">
      <CardHeader className="rounded-t-xl border-b bg-primary/[0.025]">
        <div className="flex items-start justify-between gap-3">
          <div>
            <div className="flex items-center gap-2">
              <Route className="size-4 text-primary" aria-hidden="true" />
              <h2 className="font-semibold">Store map & directions</h2>
            </div>
<<<<<<< HEAD
=======
            <p className="mt-1 text-sm text-muted-foreground">
              Explore stores and plan your trip with Google Maps.
            </p>
>>>>>>> integration
          </div>
          <Badge variant="outline">Google Maps</Badge>
        </div>
      </CardHeader>
      <CardContent className="space-y-4 pt-5">
<<<<<<< HEAD
        <form onSubmit={showStartingAddress}>
=======
        <form onSubmit={(event) => {
          event.preventDefault();
          setMapAddress(startingPoint.trim());
        }}>
>>>>>>> integration
          <div className="mb-2 flex items-center justify-between gap-2">
            <label className="text-sm font-medium" htmlFor="trip-start">Trip starting point</label>
            <Button variant="ghost" size="xs" type="button" onClick={clearStartingPoint}>Clear</Button>
          </div>
<<<<<<< HEAD
          <StartingPointInput
            key={inputReset}
            value={startingPoint}
            selectedPoint={selectedPoint}
            libraries={libraries}
            bias={selectedPoint || currentLocation}
            onChange={changeAddress}
            onSelect={selectStartingPoint}
          />
          <div className="mt-3 flex flex-wrap gap-2">
            <Button
              variant={picking ? "secondary" : "outline"}
              size="sm"
              type="button"
              disabled={!libraries}
              aria-pressed={picking && Boolean(libraries)}
              onClick={() => {
                requestVersion.current++;
                setFindingAddress(false);
                setInputReset((value) => value + 1);
                setPicking((value) => !value);
              }}
            >
              <MapPin aria-hidden="true" /> {picking ? "Cancel map selection" : "Choose on map"}
            </Button>
            <Button variant="outline" size="sm" type="submit" disabled={!startingPoint.trim() || findingAddress}>
              {findingAddress ? "Finding address…" : "Show address on map"}
            </Button>
          </div>
          {addressError && <p role="alert" className="mt-2 text-xs text-destructive">{addressError}</p>}
        </form>

        {mapsStatus !== "ready" && <p role="status" className="text-xs text-muted-foreground">
          {mapsStatus === "loading" ? "Loading Google Maps…"
            : mapsError || "Address suggestions and map selection unavailable."}
        </p>}
=======
          <div className="relative">
            <MapPin className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" aria-hidden="true" />
            <Input
              id="trip-start"
              value={startingPoint}
              onChange={(event) => setStartingPoint(event.target.value)}
              placeholder="Add your starting address"
              className="h-10 pl-9"
              maxLength={300}
            />
          </div>
          <div className="mt-2 flex items-center justify-between gap-2">
            <p className="text-xs text-muted-foreground">
              {savedOrigin ? "Uses your saved location when the address is empty." : "Enter an address, then select Show on map."}
            </p>
            <Button variant="outline" size="sm" type="submit">Show on map</Button>
          </div>
        </form>
>>>>>>> integration

        <div>
          <p className="mb-2 text-sm font-medium">Transport mode</p>
          <ToggleGroup value={[transport]} onValueChange={(value) => value[0] && setTransport(value[0])} variant="outline" size="sm" spacing={0} aria-label="Transport mode" className="grid w-full grid-cols-3">
            {transportModes.map(({ value, label, icon: Icon }) => (
              <ToggleGroupItem key={value} value={value} className="w-full justify-center gap-1.5">
                <Icon className="size-3.5" aria-hidden="true" />{label}
              </ToggleGroupItem>
            ))}
          </ToggleGroup>
        </div>

<<<<<<< HEAD
        {libraries ? (
          <StartingPointMap
            libraries={libraries}
            point={selectedPoint || currentLocation}
            picking={picking}
            onPick={selectStartingPoint}
            onCancel={() => setPicking(false)}
            onError={reportError}
          />
        ) : (
          <div className="h-80 overflow-hidden rounded-xl border bg-muted xl:h-96">
            <iframe title="Google Maps store map" src={`https://www.google.com/maps?${new URLSearchParams({ q: mapQuery, output: "embed" })}`} className="size-full border-0" loading="lazy" allowFullScreen referrerPolicy="no-referrer-when-downgrade" />
          </div>
        )}

        <p className="text-xs text-muted-foreground">Example stops</p>
        <div className="grid grid-cols-2 gap-2">
          {exampleStops.map(({ name, query }, index) => (
            <a key={query} href={`https://www.google.com/maps/search/?${new URLSearchParams({ api: "1", query })}`} target="_blank" rel="noopener noreferrer" className="rounded-lg border p-2.5 text-left hover:bg-muted focus-visible:outline-2 focus-visible:outline-ring">
              <div className="flex items-center gap-1.5 text-sm font-medium"><Store className="size-3.5 text-primary" aria-hidden="true" />{name}</div>
              <p className="mt-1 text-xs text-muted-foreground">Stop {index + 1}</p>
            </a>
          ))}
        </div>
        <Button className="w-full" nativeButton={false} render={<a href={`https://www.google.com/maps/dir/?${routeParams}`} target="_blank" rel="noopener noreferrer" />}>
=======
        <div className="h-80 overflow-hidden rounded-xl border bg-muted xl:h-96">
          <iframe
            title="Google Maps store map"
            src={`https://www.google.com/maps?${mapParams}`}
            className="size-full border-0"
            loading="lazy"
            allowFullScreen
            referrerPolicy="no-referrer-when-downgrade"
          />
        </div>

        <p className="text-xs text-muted-foreground">
          Example stops for the preview basket. Open Google Maps for current routes and travel times.
        </p>
        <div className="grid grid-cols-2 gap-2">
          {exampleStops.map(({ name, query }, index) => (
            <button key={query} type="button" onClick={() => setMapAddress(query)} className="rounded-lg border p-2.5 text-left hover:bg-muted focus-visible:outline-2 focus-visible:outline-ring">
              <div className="flex items-center gap-1.5 text-sm font-medium"><Store className="size-3.5 text-primary" aria-hidden="true" />{name}</div>
              <p className="mt-1 text-xs text-muted-foreground">Stop {index + 1} · Show on map</p>
            </button>
          ))}
        </div>
        <Button
          className="w-full"
          nativeButton={false}
          render={<a href={`https://www.google.com/maps/dir/?${routeParams}`} target="_blank" rel="noopener noreferrer" />}
        >
>>>>>>> integration
          <Navigation aria-hidden="true" /> Open route in Google Maps
        </Button>
      </CardContent>
    </Card>
  );
}
