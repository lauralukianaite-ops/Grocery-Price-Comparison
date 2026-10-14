# Grocery-Price-Comparison

> Grocery Product Price Comparison & Basket Optimizer tool
> Software Engineering I course at Vilnius University.

---

## Project Vision & Scope

This tool will be designed to track and compare food and product prices across different Lithuanian grocery stores (e.g., Barbora, LastMile/Iki). The main objective is to help consumers find the cheapest items basket and identify misleading or fake discount strategies.

---

## Core Features

1. **Background data collection** — Prices are collected in the background and updated automatically every day.
2. **Price history** — Tracks each product's price history.
3. **Cross-store product search** — Search for products across stores using a similarity index.
4. **Fake discount detection** — Detects fake or exaggerated discounts.
5. **Cheapest basket calculator** — Add items to your cart, then press "Calculate cheapest basket". The result shows the store, the item names and the total price. Each generated basket has a unique URL that can be shared.
6. **All products (indexed pages)** — Choose a store and optionally sort products by price or discount. Each product has an "Add to basket" button. This is shown on the main page by default, right below the watchlist, sorted by highest discount first.
7. **Location-based store suggestion with travel costs** — Enter either your travel cost per km, or your car's fuel consumption (litres per 100 km) and fuel price per litre, which is used to calculate your travel cost per km. The nearest stores are then found and the travel cost is added to the total cart price. You can also set a maximum travel distance in km (for example, if you're walking, you might not want to consider stores more than 2 km away). Travel is calculated as a round trip using Google Maps distances, not straight lines.
8. **Dislike product** — If a product in a generated basket isn't what you wanted, you can click a button to replace it with the next cheapest option. This may change the suggested store entirely.
9. **User account**
   1. **Watchlist and price alerts** — Add products to a watchlist, which appears on the main page when you're logged in. For each product, you can receive an alert and an email when its price reaches a set threshold.
   2. **Saved carts** — Save favorite carts.
   3. **Price discrepancy reports** — Report a difference between the price shown on the platform and the real in-store price.
   4. **Saved settings** — Save your location and transport settings.
10. **System health and monitoring dashboard** — Metrics for system health, parsing errors and performance, using Grafana and Prometheus.

---

## Branch Naming & Git Workflow

To maintain a clean repository and strict workflow, all team members must follow this Git convention.

We have two permanent branches:
* **`main`** – production branch. Always stable, containing thoroughly tested code.
* **`integration`** – primary development branch. All feature branches are merged here first to resolve conflicts.

---

### Branch Naming Rules

Always branch off from **`integration`**. Every branch must focus on a **single functionality or task** (do not modify multiple unrelated modules in one branch):

* **`feature/<short-name>`** – new feature or core functionality
  *(e.g., `feature/price-tracker`, `feature/user-auth`)*
* **`fix/<short-name>`** – bug fixes
  *(e.g., `fix/scraper-timeout`)*
* **`docs/<short-name>`** – documentation changes only
  *(e.g., `docs/update-readme`)*
* **`refactor/<short-name>`** – code optimization/cleanup without logic or feature changes
  *(e.g., `refactor/db-queries`)*

---

### Step-by-Step Development Process

1. **Create a new branch:**
   ```bash
   git checkout integration
   git pull origin integration
   git checkout -b feature/your-feature-name
2. **Commit & Push:**
   ```bash
   git add .
   git commit -m "feat: short description of changes"
   git push -u origin feature/your-feature-name
3. **Open a Pull Request (PR):**
   ```bash
   Target branch MUST be integration (never main).
   Fill out the PR description template.
   Assign at least 1 peer reviewer.
4. **Merge & Cleanup:**
   ```bash
   After 1 approval, click Merge pull request.
   Delete the branch on GitHub right after merging.
   Delete the branch locally:
      git checkout integration
      git pull origin integration
      git branch -d feature/your-feature-name
      git fetch --prune
