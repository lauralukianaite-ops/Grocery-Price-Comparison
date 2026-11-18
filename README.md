# Grocery Price Comparison

A web app for comparing grocery prices across Lithuanian stores. Users build a basket and get the store where it costs the least, including the cost of travel. Users can also exclude unwanted products or share the result through a unique link. Products can be searched and browsed across stores, with each product’s price history and a check on whether its discount is real. Registered users can keep a watchlist with price alerts, save baskets and report price discrepancies.

## Stores Supported

Prices are collected from Maxima, IKI, PROMO Cash&Carry, Rimi and 16 more store chains.

## Core Features

1. **Background data collection** — Prices are collected in the background and updated automatically every day.
1. **Price history** — See each product's price history.
1. **Cross-store product search** — Search for products across stores using a similarity index.
1. **Fake discount detection** — Detects fake or exaggerated discounts. You can see flags on the main page and in your generated basket.
1. **Cheapest basket calculator** — Add items to your basket, then press "Calculate cheapest basket". The result shows the store, the item names and the total price. Next to the total price, you can see the cost of groceries and the cost of travel for a fuller picture. Each generated basket has a unique URL that can be copied to clipboard and shared.
1. **All products (indexed pages)** — Filter by store and sort by price or discount percentage. Each product has an "Add to basket" button. This is shown on the main page, sorted by highest discount first by default.
1. **Location-based store suggestion with travel costs** — Enter either your travel cost per km, or your car's fuel consumption (litres per 100 km) and fuel price per litre, which is used to calculate your travel cost per km. The nearest stores are then found and the travel cost is added to the total basket price. You can also set a maximum travel distance in km (for example, if you're walking, you might not want to consider stores more than 2 km away). Travel is calculated as a round trip using Google Maps distances, not straight lines.
1. **Dislike product** — If a product in a generated basket isn't what you wanted, you can click a button to recalculate and find the next cheapest basket that does not include the disliked product. This may change the suggested store entirely.
1. **User account** — Users sign in with Auth0. Watchlists, saved baskets, saved settings and submitting price reports require a signed-in user. Everything else on the site, including reading price reports, works without an account.
   1. **Watchlist and price alerts** — On any product, a signed-in user can click "Add to watchlist" and set a target price. A background job checks the watchlist after each price update. When a product's newest price is at or below the target, the user gets an in-app alert and an email. Each target triggers once per price drop, not on every check, and the user can edit or remove a target at any time.
   1. **Saved baskets** — Any generated basket can be saved to the user's account with a name. The saved list shows each basket's store, total and the date it was generated. Opening one shows the same snapshot as its shareable link. From there the user can recalculate the basket with current prices or delete it.
   1. **Price discrepancy reports** — A signed-in user can report that a product's price in the store differs from the one on the site by entering the price they saw. Each user can report a given product once per day. Reading reports needs no account: every product, in the catalogue and in a generated basket, shows a person icon with the number of reports from the last 7 days. Clicking it opens the list of those reports, one row each with date, time and reported price (for example "Nov 13, 14:55, 6.50 EUR").
   1. **Saved settings** — Users can save a default starting location and travel settings: cost per km or fuel consumption and fuel price, the maximum travel distance and the maximum number of stores to visit. The basket form fills in these values automatically, and they can still be changed for a single calculation without overwriting the saved ones.
1. **System health and monitoring dashboard** — Metrics for system health, parsing errors and performance, using Grafana and Prometheus.

## Tech Stack

- **Backend:** ASP.NET Core (.NET 10), Entity Framework Core
- **Scraper:** .NET 10 Worker Service, Playwright
- **Frontend:** React 19, Vite, Tailwind CSS, shadcn/ui
- **Database:** PostgreSQL
- **Tests:** xUnit, Moq

## For Contributors

To maintain a clean repository and strict workflow, all team members must follow this Git convention.

We have two permanent branches:
* **`main`** – production branch. Always stable, containing thoroughly tested code.
* **`integration`** – primary development branch. All feature branches are merged here first to resolve conflicts.

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
