export const GOOGLE_MAPS_API_KEY = import.meta.env?.VITE_GOOGLE_MAPS_API_KEY?.trim() || "";
export const GOOGLE_MAPS_MAP_ID = import.meta.env?.VITE_GOOGLE_MAPS_MAP_ID?.trim() || "DEMO_MAP_ID";
export const GOOGLE_MAPS_AUTH_ERROR = "pricebasket:google-maps-auth-error";
export const DEFAULT_MAP_CENTER = { lat: 54.6872, lng: 25.2797 };

let mapsPromise;

export function getCoordinates(point) {
  if (!Number.isFinite(point?.latitude) || !Number.isFinite(point?.longitude) ||
    Math.abs(point.latitude) > 90 || Math.abs(point.longitude) > 180) {
    return null;
  }

  return { lat: point.latitude, lng: point.longitude };
}

export function getStartingPointQuery(point) {
  const coordinates = getCoordinates(point);
  return coordinates
    ? `${coordinates.lat},${coordinates.lng}`
    : point?.address?.trim() || "";
}

export async function resolvePlacePrediction(prediction) {
  const place = prediction.toPlace();
  await place.fetchFields({ fields: ["formattedAddress", "location"] });

  if (!place.location) {
    throw new Error("This address has no map location. Try another suggestion.");
  }

  return {
    address: place.formattedAddress || prediction.text.toString(),
    latitude: place.location.lat(),
    longitude: place.location.lng(),
  };
}

export async function reverseGeocodePoint(geocoder, coordinates) {
  let address = `${coordinates.lat.toFixed(6)}, ${coordinates.lng.toFixed(6)}`;

  try {
    const { results } = await geocoder.geocode({ location: coordinates });
    address = results[0]?.formatted_address || address;
  } catch {
    // Keep coordinates when no address is available.
  }

  return { address, latitude: coordinates.lat, longitude: coordinates.lng };
}

export function loadGoogleMaps() {
  if (!GOOGLE_MAPS_API_KEY) {
    return Promise.reject(new Error("Google Maps is not configured."));
  }

  if (!mapsPromise) {
    mapsPromise = new Promise((resolve, reject) => {
      if (window.google?.maps?.importLibrary) {
        resolve(window.google.maps);
        return;
      }

      const script = document.createElement("script");
      const callback = "__pricebasketGoogleMapsReady";
      const previousAuthFailure = window.gm_authFailure;
      let settled = false;
      let timeout;

      function finish(error) {
        if (settled) return;
        settled = true;
        window.clearTimeout(timeout);
        delete window[callback];
        if (error) {
          script.remove();
          window.gm_authFailure = previousAuthFailure;
          reject(error);
        } else {
          resolve(window.google.maps);
        }
      }

      window[callback] = () => finish();
      window.gm_authFailure = () => {
        window.dispatchEvent(new Event(GOOGLE_MAPS_AUTH_ERROR));
        finish(new Error("Google Maps could not authorize this website."));
        previousAuthFailure?.();
      };
      script.src = `https://maps.googleapis.com/maps/api/js?${new URLSearchParams({
        key: GOOGLE_MAPS_API_KEY,
        v: "weekly",
        loading: "async",
        callback,
        region: "LT",
        language: "en",
      })}`;
      script.async = true;
      script.onerror = () => finish(new Error("Google Maps could not load. Check your connection."));
      timeout = window.setTimeout(() => finish(new Error("Google Maps took too long to load.")), 15000);
      document.head.append(script);
    }).then(async (maps) => {
      const [mapLibrary, places, marker, geocoding] = await Promise.all([
        maps.importLibrary("maps"),
        maps.importLibrary("places"),
        maps.importLibrary("marker"),
        maps.importLibrary("geocoding"),
      ]);
      return { ...mapLibrary, ...places, ...marker, ...geocoding };
    }).catch((error) => {
      mapsPromise = undefined;
      throw error;
    });
  }

  return mapsPromise;
}
