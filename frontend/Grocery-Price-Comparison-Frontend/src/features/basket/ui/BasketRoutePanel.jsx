import { useState } from "react";
import { Bike, Car, Footprints, MapPin, Navigation, Route, Store } from "lucide-react";
import { Button } from "../../../components/ui/button";
import { Input } from "../../../components/ui/input";
import { Badge } from "../../../components/ui/badge";
import { Card, CardContent, CardHeader } from "../../../components/ui/card";
import { ToggleGroup, ToggleGroupItem } from "../../../components/ui/toggle-group";

const transportModes = [
  { value: "car", label: "Car", icon: Car },
  { value: "bike", label: "Bike", icon: Bike },
  { value: "walk", label: "Walk", icon: Footprints },
];

export default function BasketRoutePanel() {
  const [transport, setTransport] = useState("car");
  const [startingPoint, setStartingPoint] = useState("");

  return (
    <Card className="h-fit overflow-hidden border-primary/15 shadow-sm xl:sticky xl:top-5">
      <CardHeader className="border-b bg-primary/[0.025]">
        <div className="flex items-start justify-between gap-3">
          <div>
            <div className="flex items-center gap-2">
              <Route className="size-4 text-primary" aria-hidden="true" />
              <h2 className="font-semibold">Live route & store map</h2>
            </div>
            <p className="mt-1 text-sm text-muted-foreground">
              Plan the trip around your selected stores.
            </p>
          </div>
          <Badge variant="outline">Preview</Badge>
        </div>
      </CardHeader>
      <CardContent className="space-y-4 pt-5">
        <div>
          <div className="mb-2 flex items-center justify-between gap-2">
            <label className="text-sm font-medium" htmlFor="trip-start">Trip starting point</label>
            <Button variant="ghost" size="xs" type="button" onClick={() => setStartingPoint("")}>Clear</Button>
          </div>
          <div className="relative">
            <MapPin className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" aria-hidden="true" />
            <Input
              id="trip-start"
              value={startingPoint}
              onChange={(event) => setStartingPoint(event.target.value)}
              placeholder="Add your starting address"
              className="h-10 pl-9"
            />
          </div>
        </div>

        <div>
          <p className="mb-2 text-sm font-medium">Transport mode</p>
          <ToggleGroup
            value={[transport]}
            onValueChange={(value) => value[0] && setTransport(value[0])}
            variant="outline"
            size="sm"
            spacing={0}
            aria-label="Transport mode"
            className="grid w-full grid-cols-3"
          >
            {transportModes.map(({ value, label, icon: Icon }) => (
              <ToggleGroupItem key={value} value={value} className="w-full justify-center gap-1.5">
                <Icon className="size-3.5" aria-hidden="true" />
                {label}
              </ToggleGroupItem>
            ))}
          </ToggleGroup>
        </div>

        <div className="relative h-64 overflow-hidden rounded-xl border bg-[radial-gradient(circle_at_18%_18%,hsl(var(--primary)/0.16)_0,transparent_28%),radial-gradient(circle_at_78%_72%,hsl(var(--accent)/0.6)_0,transparent_34%),linear-gradient(135deg,hsl(var(--muted))_0%,hsl(var(--background))_100%)] xl:h-72">
          <div className="absolute -left-8 top-16 h-14 w-[130%] -rotate-6 rounded-full border-y border-primary/15 bg-background/35" />
          <div className="absolute -left-10 top-28 h-8 w-[130%] rotate-12 rounded-full border-y border-primary/10 bg-background/45" />
          <svg className="absolute inset-0 size-full" viewBox="0 0 420 230" aria-label="Route preview map" role="img">
            <path d="M52 174 L124 143 L205 152 L274 88 L358 122" fill="none" stroke="hsl(var(--primary))" strokeWidth="5" strokeDasharray="8 7" strokeLinecap="round" />
            <circle cx="52" cy="174" r="10" fill="hsl(var(--primary))" />
            <circle cx="205" cy="152" r="10" fill="hsl(var(--destructive))" />
            <circle cx="358" cy="122" r="10" fill="hsl(221 83% 53%)" />
          </svg>
          <span className="absolute bottom-3 left-3 rounded-md bg-background/90 px-2 py-1 text-xs font-medium shadow-sm">Start</span>
          <span className="absolute bottom-[3.6rem] left-[42%] rounded-md bg-destructive px-2 py-1 text-xs font-medium text-destructive-foreground shadow-sm">Rimi</span>
          <span className="absolute right-3 top-[4.4rem] rounded-md bg-blue-600 px-2 py-1 text-xs font-medium text-white shadow-sm">Lidl</span>
          <div className="absolute inset-x-3 bottom-3 flex items-center justify-between rounded-lg border bg-background/95 p-2.5 shadow-sm">
            <span className="text-xs text-muted-foreground"><strong className="text-foreground">4.2 km</strong> · 14 min by {transport}</span>
            <Navigation className="size-4 text-primary" aria-hidden="true" />
          </div>
        </div>

        <div className="grid grid-cols-2 gap-2">
          {[["Rimi Hyper", "Open · 3 min"], ["Lidl Žirmūnai", "Open · 8 min"]].map(([name, status]) => (
            <div key={name} className="rounded-lg border p-2.5">
              <div className="flex items-center gap-1.5 text-sm font-medium"><Store className="size-3.5 text-primary" aria-hidden="true" />{name}</div>
              <p className="mt-1 text-xs text-emerald-700">{status}</p>
            </div>
          ))}
        </div>
        <Button className="w-full" type="button"><Navigation aria-hidden="true" /> Open route in maps</Button>
      </CardContent>
    </Card>
  );
}
