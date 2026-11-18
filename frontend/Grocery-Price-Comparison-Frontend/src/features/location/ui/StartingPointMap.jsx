import { useEffect, useRef, useState } from "react";
import { DEFAULT_MAP_CENTER, GOOGLE_MAPS_MAP_ID, getCoordinates, reverseGeocodePoint } from "../api/googleMaps";

export default function StartingPointMap({ libraries, point, picking, onPick, onCancel, onError }) {
  const container = useRef(null);
  const mapRef = useRef(null);
  const markerRef = useRef(null);
  const requestVersion = useRef(0);
  const selectCenterRef = useRef(null);
  const initialCenter = useRef(getCoordinates(point) || DEFAULT_MAP_CENTER);
  const [status, setStatus] = useState("");

  useEffect(() => {
    let map = mapRef.current;
    let marker = markerRef.current;
    try {
      // Reuse the map across Strict Mode effect replays.
      map ??= new libraries.Map(container.current, {
        center: initialCenter.current,
        zoom: 13,
        mapId: GOOGLE_MAPS_MAP_ID,
        mapTypeControl: false,
        streetViewControl: false,
        fullscreenControl: true,
        gestureHandling: "cooperative",
        clickableIcons: false,
      });
      marker ??= new libraries.AdvancedMarkerElement({ title: "Trip starting point" });
      marker.map = map;
    } catch {
      onError();
      return;
    }
    mapRef.current = map;
    markerRef.current = marker;
    return () => {
      requestVersion.current++;
      try {
        marker.map = null;
      } catch {
        // Google may dispose the renderer before cleanup.
      }
    };
  }, [libraries, onError]);

  useEffect(() => {
    if (!mapRef.current || !markerRef.current) return;
    const coordinates = getCoordinates(point);
    markerRef.current.position = coordinates || undefined;
    if (coordinates) mapRef.current.panTo(coordinates);
  }, [point, libraries]);

  useEffect(() => {
    const map = mapRef.current;
    const marker = markerRef.current;
    if (!map || !marker) return;
    map.setOptions({ draggableCursor: picking ? "crosshair" : null });
    if (!picking) return;
    setStatus("");
    const geocoder = new libraries.Geocoder();

    async function selectPoint(coordinates) {
      const version = ++requestVersion.current;
      marker.position = coordinates;
      setStatus("Finding the address…");
      const selectedPoint = await reverseGeocodePoint(geocoder, coordinates);
      if (version !== requestVersion.current) return;
      setStatus("");
      onPick(selectedPoint);
    }

    const listener = map.addListener("click", (event) => {
      if (event.latLng) selectPoint({ lat: event.latLng.lat(), lng: event.latLng.lng() });
    });
    const selectCenter = () => {
      const center = map.getCenter();
      if (center) selectPoint({ lat: center.lat(), lng: center.lng() });
    };
    selectCenterRef.current = selectCenter;

    return () => {
      requestVersion.current++;
      selectCenterRef.current = null;
      try {
        listener.remove();
        marker.position = getCoordinates(point) || undefined;
      } catch {
        // The map may already be disposed.
      }
    };
  }, [picking, onPick, point, libraries]);

  return (
    <div className="space-y-2">
      {picking && (
        <div className="flex flex-wrap items-center justify-between gap-2 rounded-lg border border-primary/20 bg-muted p-3 text-xs">
          <span role="status">{status || "Selecting starting point"}</span>
          <div className="flex gap-3">
            <button type="button" onClick={() => selectCenterRef.current?.()} className="font-medium underline underline-offset-4">Use map center</button>
            <button type="button" onClick={onCancel} className="font-medium underline underline-offset-4">Cancel</button>
          </div>
        </div>
      )}
      <div ref={container} aria-label={picking ? "Choose a starting point on Google Maps" : "Google Maps store map"} className="relative z-0 h-80 overflow-hidden rounded-xl border bg-muted xl:h-96" />
    </div>
  );
}
