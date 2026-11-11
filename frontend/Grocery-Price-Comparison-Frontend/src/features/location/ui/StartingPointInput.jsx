import { useEffect, useId, useRef, useState } from "react";
import { LoaderCircle, MapPin } from "lucide-react";
import { Input } from "../../../components/ui/input";
import { DEFAULT_MAP_CENTER, resolvePlacePrediction } from "../api/googleMaps";

export default function StartingPointInput({ value, selectedPoint, libraries, bias, onChange, onSelect }) {
  const listId = useId();
  const [suggestions, setSuggestions] = useState([]);
  const [suggestionQuery, setSuggestionQuery] = useState("");
  const [status, setStatus] = useState("idle");
  const [open, setOpen] = useState(false);
  const [focused, setFocused] = useState(false);
  const [activeIndex, setActiveIndex] = useState(-1);
  const requestVersion = useRef(0);
  const selectionVersion = useRef(0);
  const sessionToken = useRef(null);
  const latitude = bias?.latitude ?? DEFAULT_MAP_CENTER.lat;
  const longitude = bias?.longitude ?? DEFAULT_MAP_CENTER.lng;

  useEffect(() => () => {
    selectionVersion.current++;
  }, [value, selectedPoint, libraries]);

  useEffect(() => {
    const version = ++requestVersion.current;
    if (!libraries || !focused || selectedPoint || value.trim().length < 3) return;

    const timer = window.setTimeout(async () => {
      setStatus("searching");
      try {
        sessionToken.current ??= new libraries.AutocompleteSessionToken();
        const { suggestions: results } = await libraries.AutocompleteSuggestion.fetchAutocompleteSuggestions({
          input: value.trim(),
          sessionToken: sessionToken.current,
          region: "lt",
          locationBias: { center: { lat: latitude, lng: longitude }, radius: 50000 },
        });
        if (version !== requestVersion.current) return;
        setSuggestions(results.flatMap((result) => result.placePrediction ? [result.placePrediction] : []));
        setSuggestionQuery(value);
        setActiveIndex(-1);
        setStatus("results");
      } catch {
        if (version !== requestVersion.current) return;
        setSuggestions([]);
        setStatus("error");
      }
    }, 300);

    return () => {
      window.clearTimeout(timer);
      requestVersion.current++;
    };
  }, [value, selectedPoint, libraries, latitude, longitude, focused]);

  async function selectPrediction(prediction) {
    requestVersion.current++;
    const version = ++selectionVersion.current;
    setOpen(false);
    setSuggestions([]);
    setStatus("selecting");
    sessionToken.current = null;
    try {
      const point = await resolvePlacePrediction(prediction);
      if (version !== selectionVersion.current) return;
      setStatus("idle");
      onSelect(point);
    } catch {
      if (version !== selectionVersion.current) return;
      setStatus("error");
    }
  }

  const liveSuggestions = selectedPoint || suggestionQuery !== value ? [] : suggestions;
  const showSuggestions = open && liveSuggestions.length > 0;
  const isLoading = libraries && !selectedPoint && ((focused && status === "searching") || status === "selecting");
  const statusMessage = selectedPoint || !libraries ? ""
    : status === "error" ? "Address lookup unavailable."
    : status === "selecting" ? "Finding address…"
    : status === "searching" && focused ? "Searching addresses…"
    : open && status === "results" && suggestionQuery === value && !suggestions.length ? "No addresses found."
    : "";

  function handleKeyDown(event) {
    if ((event.key === "ArrowDown" || event.key === "ArrowUp") && liveSuggestions.length) {
      event.preventDefault();
      setOpen(true);
      setActiveIndex((index) => event.key === "ArrowDown"
        ? (index + 1) % liveSuggestions.length
        : (index <= 0 ? liveSuggestions.length : index) - 1);
    } else if (event.key === "Enter" && showSuggestions && activeIndex >= 0) {
      event.preventDefault();
      selectPrediction(liveSuggestions[activeIndex]);
    } else if (event.key === "Escape") {
      event.preventDefault();
      setOpen(false);
    }
  }

  return (
    <div onBlur={(event) => {
      if (!event.currentTarget.contains(event.relatedTarget)) {
        setOpen(false);
        setFocused(false);
      }
    }}>
      <div className="relative">
        <MapPin className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" aria-hidden="true" />
        <Input
          id="trip-start"
          role="combobox"
          aria-autocomplete="list"
          aria-expanded={showSuggestions}
          aria-controls={showSuggestions ? listId : undefined}
          aria-activedescendant={showSuggestions && activeIndex >= 0 ? `${listId}-${activeIndex}` : undefined}
          aria-describedby={statusMessage ? `${listId}-status` : undefined}
          autoComplete="off"
          value={value}
          placeholder="Search for a starting address"
          maxLength={300}
          onFocus={() => {
            setOpen(true);
            setFocused(true);
          }}
          onKeyDown={handleKeyDown}
          onChange={(event) => {
            requestVersion.current++;
            selectionVersion.current++;
            setSuggestions([]);
            setActiveIndex(-1);
            setStatus("idle");
            setOpen(true);
            if (!event.target.value.trim()) sessionToken.current = null;
            onChange(event.target.value);
          }}
          className="h-10 pl-9 pr-9"
        />
        {isLoading && <LoaderCircle className="absolute right-3 top-1/2 size-4 -translate-y-1/2 animate-spin text-muted-foreground" aria-hidden="true" />}
        {showSuggestions && (
          <ul id={listId} role="listbox" aria-label="Suggested starting addresses" className="absolute inset-x-0 top-full z-20 mt-1 max-h-64 overflow-y-auto rounded-lg border bg-popover p-1 text-popover-foreground shadow-lg">
            {liveSuggestions.map((prediction, index) => (
              <li
                key={prediction.placeId}
                id={`${listId}-${index}`}
                role="option"
                aria-selected={activeIndex === index}
                className={`cursor-pointer rounded-md px-3 py-2 text-sm ${activeIndex === index ? "bg-accent text-accent-foreground" : "hover:bg-muted"}`}
                onMouseDown={(event) => event.preventDefault()}
                onMouseMove={() => setActiveIndex(index)}
                onClick={() => selectPrediction(prediction)}
              >
                {prediction.text.toString()}
              </li>
            ))}
            <li role="presentation" className="border-t px-3 py-2 text-right text-xs text-muted-foreground">Google Maps</li>
          </ul>
        )}
      </div>
      {statusMessage && <p id={`${listId}-status`} role="status" className="mt-2 text-xs text-muted-foreground">{statusMessage}</p>}
    </div>
  );
}
