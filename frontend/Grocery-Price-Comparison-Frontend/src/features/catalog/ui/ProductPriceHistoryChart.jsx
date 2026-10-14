import { TrendingUp } from "lucide-react";
import {
  CartesianGrid,
  Line,
  LineChart,
  XAxis,
  YAxis,
} from "recharts";
import { Badge } from "../../../components/ui/badge";
import {
  Card,
  CardContent,
  CardHeader,
} from "../../../components/ui/card";
import {
  ChartContainer,
  ChartLegend,
  ChartLegendContent,
  ChartTooltip,
  ChartTooltipContent,
} from "../../../components/ui/chart";
import { formatPrice } from "../productFormatting";

const SERIES_COLORS = [
  "#171717",
  "#16a34a",
  "#2563eb",
  "#ea580c",
  "#9333ea",
  "#0891b2",
];

const shortDateFormatter = new Intl.DateTimeFormat("en", {
  day: "numeric",
  month: "short",
});

const longDateFormatter = new Intl.DateTimeFormat("en", {
  day: "numeric",
  month: "long",
  year: "numeric",
});

function parseDate(value) {
  if (!value) return null;

  const date = new Date(
    /^\d{4}-\d{2}-\d{2}$/.test(value) ? `${value}T00:00:00` : value,
  );

  return Number.isNaN(date.getTime()) ? null : date;
}

function formatDate(value, formatter) {
  const date = parseDate(value);
  return date ? formatter.format(date) : "Unknown date";
}

function getDateKey(value) {
  if (/^\d{4}-\d{2}-\d{2}$/.test(value)) return value;

  const date = parseDate(value);
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");

  return `${year}-${month}-${day}`;
}

function buildChartModel(priceHistory) {
  const sortedHistory = [...priceHistory]
    .filter(
      (point) =>
        Number.isFinite(Number(point.cost)) && parseDate(point.recordedAt),
    )
    .sort(
      (left, right) =>
        parseDate(left.recordedAt).getTime() -
        parseDate(right.recordedAt).getTime(),
    );
  const stores = [];
  const storeIndexes = new Map();

  sortedHistory.forEach((point) => {
    const storeIdentity = String(point.storeId ?? point.storeName);

    if (!storeIndexes.has(storeIdentity)) {
      storeIndexes.set(storeIdentity, stores.length);
      stores.push({
        id: storeIdentity,
        key: `store${stores.length + 1}`,
        name: point.storeName,
      });
    }
  });

  const rowsByDate = new Map();

  sortedHistory.forEach((point) => {
    const dateKey = getDateKey(point.recordedAt);
    const storeIndex = storeIndexes.get(String(point.storeId ?? point.storeName));
    const store = stores[storeIndex];
    const row = rowsByDate.get(dateKey) ?? { recordedAt: dateKey };

    row[store.key] = Number(point.cost);
    rowsByDate.set(dateKey, row);
  });

  const series = stores.map((store, index) => ({
    ...store,
    color: SERIES_COLORS[index % SERIES_COLORS.length],
  }));

  return {
    data: [...rowsByDate.values()],
    series,
    config: Object.fromEntries(
      series.map((store) => [
        store.key,
        { label: store.name, color: store.color },
      ]),
    ),
  };
}

export default function ProductPriceHistoryChart({ priceHistory = [] }) {
  const { data, series, config } = buildChartModel(priceHistory);

  return (
    <section className="mt-10" aria-labelledby="price-history-title">
      <Card>
        <CardHeader className="flex flex-row items-start justify-between gap-4">
          <div>
            <h2
              id="price-history-title"
              className="flex items-center gap-2 text-base font-medium leading-snug"
            >
              <TrendingUp className="size-5" aria-hidden="true" />
              Price history
            </h2>
            <p className="mt-1 text-sm text-muted-foreground">
              Track how this product&apos;s listed price changes over time.
            </p>
          </div>
          {data.length > 0 && (
            <Badge variant="secondary">
              {data.length} {data.length === 1 ? "date" : "dates"}
            </Badge>
          )}
        </CardHeader>
        <CardContent>
          {data.length > 1 ? (
            <ChartContainer
              config={config}
              className="h-72 w-full aspect-auto sm:h-80"
            >
              <LineChart
                accessibilityLayer
                data={data}
                margin={{ top: 8, right: 12, left: 8, bottom: 8 }}
              >
                <CartesianGrid vertical={false} />
                <XAxis
                  dataKey="recordedAt"
                  axisLine={false}
                  tickLine={false}
                  minTickGap={28}
                  tickFormatter={(value) =>
                    formatDate(value, shortDateFormatter)
                  }
                />
                <YAxis
                  axisLine={false}
                  tickLine={false}
                  width={64}
                  domain={["auto", "auto"]}
                  tickFormatter={formatPrice}
                />
                <ChartTooltip
                  cursor={false}
                  content={
                    <ChartTooltipContent
                      labelFormatter={(_, payload) =>
                        formatDate(
                          payload?.[0]?.payload?.recordedAt,
                          longDateFormatter,
                        )
                      }
                      formatter={(value, name) => (
                        <div className="flex min-w-36 flex-1 items-center justify-between gap-4">
                          <span className="text-muted-foreground">
                            {config[name]?.label ?? name}
                          </span>
                          <span className="font-mono font-medium text-foreground tabular-nums">
                            {formatPrice(value)}
                          </span>
                        </div>
                      )}
                    />
                  }
                />
                <ChartLegend content={<ChartLegendContent />} />
                {series.map((store) => (
                  <Line
                    key={store.key}
                    dataKey={store.key}
                    type="monotone"
                    stroke={`var(--color-${store.key})`}
                    strokeWidth={2}
                    dot={{ r: 3 }}
                    activeDot={{ r: 5 }}
                    connectNulls
                  />
                ))}
              </LineChart>
            </ChartContainer>
          ) : (
            <div className="rounded-lg border border-dashed px-4 py-12 text-center">
              <p className="text-sm font-medium">No price history yet</p>
              <p className="mt-1 text-sm text-muted-foreground">
                Historical prices from the catalog API will appear here.
              </p>
            </div>
          )}
        </CardContent>
      </Card>
    </section>
  );
}
