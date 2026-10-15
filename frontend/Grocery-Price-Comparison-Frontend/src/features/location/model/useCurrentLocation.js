import { useCallback, useState } from "react";
import { useLocationStore } from "./locationStore";

const GEOLOCATION_OPTIONS = Object.freeze({
  enableHighAccuracy: false,
  maximumAge: 5 * 60 * 1000,
  timeout: 10 * 1000,
});

function getLocationErrorMessage(error) {
  if (error?.code === 1) {
    return "Location access was declined. You can allow it in your browser settings.";
  }

  if (error?.code === 2) {
    return "Your current location could not be determined.";
  }

  if (error?.code === 3) {
    return "Finding your location took too long. Please try again.";
  }

  return "Your location could not be saved. Please try again.";
}

export function useCurrentLocation() {
  const currentLocation = useLocationStore((state) => state.currentLocation);
  const saveCurrentLocation = useLocationStore(
    (state) => state.saveCurrentLocation,
  );
  const clearStoredLocation = useLocationStore(
    (state) => state.clearCurrentLocation,
  );
  const [status, setStatus] = useState("idle");
  const [error, setError] = useState("");

  const requestCurrentLocation = useCallback(() => {
    if (!navigator.geolocation) {
      setStatus("error");
      setError("Location services are not supported by this browser.");
      return;
    }

    setStatus("requesting");
    setError("");

    navigator.geolocation.getCurrentPosition(
      (position) => {
        saveCurrentLocation({
          latitude: Number(position.coords.latitude.toFixed(6)),
          longitude: Number(position.coords.longitude.toFixed(6)),
          accuracy: Math.round(position.coords.accuracy),
          savedAt: new Date().toISOString(),
        });
        setStatus("success");
      },
      (locationError) => {
        setStatus("error");
        setError(getLocationErrorMessage(locationError));
      },
      GEOLOCATION_OPTIONS,
    );
  }, [saveCurrentLocation]);

  const clearCurrentLocation = useCallback(() => {
    clearStoredLocation();
    setStatus("idle");
    setError("");
  }, [clearStoredLocation]);

  return {
    currentLocation,
    error,
    isRequesting: status === "requesting",
    requestCurrentLocation,
    clearCurrentLocation,
  };
}
