import { useCallback, useEffect, useState } from "react";
import { GOOGLE_MAPS_API_KEY, GOOGLE_MAPS_AUTH_ERROR, loadGoogleMaps } from "../api/googleMaps";

export function useGoogleMaps() {
  const [state, setState] = useState({
    libraries: null,
    status: GOOGLE_MAPS_API_KEY ? "loading" : "unconfigured",
    error: "",
  });
  const reportError = useCallback(() => {
    setState({ libraries: null, status: "error", error: "Google Maps is unavailable." });
  }, []);

  useEffect(() => {
    if (!GOOGLE_MAPS_API_KEY) return;
    let cancelled = false;
    let authorizationFailed = false;
    function handleAuthError() {
      authorizationFailed = true;
      reportError();
    }
    window.addEventListener(GOOGLE_MAPS_AUTH_ERROR, handleAuthError);
    loadGoogleMaps().then(
      (libraries) => {
        if (!cancelled && !authorizationFailed) setState({ libraries, status: "ready", error: "" });
      },
      () => {
        if (!cancelled) reportError();
      },
    );
    return () => {
      cancelled = true;
      window.removeEventListener(GOOGLE_MAPS_AUTH_ERROR, handleAuthError);
    };
  }, [reportError]);

  return { ...state, reportError };
}
