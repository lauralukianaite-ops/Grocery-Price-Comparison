# Grocery-Price-Comparison

> Grocery Product Price Comparison & Basket Optimizer tool
> Software Engineering I course at Vilnius University.

---

## Project Vision & Scope

This tool will be designed to track and compare food and product prices across different Lithuanian grocery stores (e.g., Barbora, LastMile/Iki). The main objective is to help consumers find the cheapest items basket and identify misleading or fake discount strategies.

---

## 10 Core Features

1. **Cross-Store Product Search:** Search for products by name or category across multiple supported stores.
2. **Real-Time Price Comparison:** View current prices for the same product in different stores side-by-side.
3. **Price History Tracking:** Store price data to monitor price changes over time.
4. **Fake Discount Detection:** Identify suspicious discounts where prices were raised right before a promotion.
5. **Product Price Status Flagging:** Mark items with clear visual flags (e.g., `Normal`, `SuspiciousDiscount`, `PriceSurge`).
6. **Background Data Collection:** Automatically fetch and refresh latest prices periodically in the background.
7. **User Watchlist & Price Alerts:** Allow users to save favorite items to a personalized watchlist and get notified when prices drop.
8. **Cheapest Basket Calculator:** Calculate which store offers the lowest total price for a full user shopping list.
9. **System health and monitoring dashboard:** provide metrics for monitoring system health, parsing errors, and performance.
10. **User Price Reporting Feature:** Allows any user to report a price discrepancy between the platform and the retailer.

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