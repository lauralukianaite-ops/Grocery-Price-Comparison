import assert from "node:assert/strict";
import test from "node:test";
import { getCoordinates, getStartingPointQuery, resolvePlacePrediction, reverseGeocodePoint } from "../src/features/location/api/googleMaps.js";

test("directions preserve the exact clicked coordinates rather than the nearest street address", async () => {
  const point = await reverseGeocodePoint({
    geocode: async () => ({ results: [{ formatted_address: "Nearby street, Vilnius" }] }),
  }, { lat: 54.687234567, lng: 25.279712345 });
  assert.equal(point.address, "Nearby street, Vilnius");
  assert.equal(getStartingPointQuery(point), "54.687234567,25.279712345");
});

test("an unaddressed pin remains usable when geocoding fails or returns no results", async () => {
  for (const geocode of [async () => ({ results: [] }), async () => { throw new Error("REQUEST_DENIED"); }]) {
    const point = await reverseGeocodePoint({ geocode }, { lat: 54.687234567, lng: 25.279712345 });
    assert.equal(point.address, "54.687235, 25.279712");
    assert.equal(getStartingPointQuery(point), "54.687234567,25.279712345");
  }
});

test("manual addresses and zero-valued coordinates remain valid route origins", () => {
  assert.equal(getStartingPointQuery({ latitude: 0, longitude: 0 }), "0,0");
  assert.equal(getStartingPointQuery({ address: "  Žirmūnų g. 10, Vilnius  " }), "Žirmūnų g. 10, Vilnius");
  assert.equal(getStartingPointQuery(null), "");
  for (const point of [{ latitude: NaN, longitude: 25 }, { latitude: 91, longitude: 25 }, { latitude: 54, longitude: 181 }]) {
    assert.equal(getCoordinates(point), null);
  }
});

test("a selected prediction resolves its coordinates and rejects places without a location", async () => {
  const prediction = {
    text: { toString: () => "Vilnius Cathedral" },
    toPlace: () => ({
      fetchFields: async () => {},
      formattedAddress: "Cathedral Square, Vilnius",
      location: { lat: () => 54.6858, lng: () => 25.2874 },
    }),
  };
  assert.deepEqual(await resolvePlacePrediction(prediction), {
    address: "Cathedral Square, Vilnius", latitude: 54.6858, longitude: 25.2874,
  });
  prediction.toPlace = () => ({ fetchFields: async () => {} });
  await assert.rejects(resolvePlacePrediction(prediction), /no map location/);
});
