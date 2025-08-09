UbiSim — An Economically-Grounded UBI Simulator (C#)

UbiSim is a **deterministic, reproducible** macro-micro simulation that explores the fiscal and real-economy impacts of a Universal Basic Income (UBI) program. It models **households, firms, labor markets, finance, trade, and regions**, calibrates **tax instruments** to a fiscal target, then runs multi-month scenarios and exports **rich CSV outputs**.

---

## Highlights

* **End-to-end UBI scenarios**: No-UBI baseline and multiple calibrated UBI levels.
* **Automatic tax calibration**: Searches corporate, PIT scale, and VAT to hit a net fiscal target with stability checks.
* **Bounded, stable dynamics**: Tight clamps on inflation, interest rates, GDP, and migration to avoid blowups.
* **Reproducible**: Seeded RNG + option to run deterministic (no stochastic sampling).
* **CSV exports**: Monthly series per scenario + summary across scenarios for post-analysis.

---

## Quick Start

### Prerequisites

* .NET 8 SDK (or .NET 7; project uses modern C# features like `record`)
* Windows, macOS, or Linux

### Build & Run

```bash
# from the solution/project folder
dotnet build -c Release

# Run a single scenario for 24 months with UBI=$1,000, write monthly CSV
dotnet run --project . -- --ubi 1000 --months 24 --csv out/monthly_ubi1000.csv --stochastic false --advanced true
```

> Note the `--` separator before app args.

### Typical Outputs

* Console summary (first/last 6 months + totals)
* `monthly.csv` (if `--csv` given): per-month indicators
* `enhanced_out/enhanced_scenario_summary.csv` when running the built-in scenario suite

---

## How to Use

### Single Scenario (custom UBI & target)

```bash
dotnet run -- --ubi 1200 --months 18 --target-net 0 \
  --csv out/ubi1200_monthly.csv --seed 42 --stochastic true --advanced true
```

### Scenario Suite (predefined set; writes to `enhanced_out/`)

```bash
dotnet run -- --months 12 --stochastic false --advanced true
# If --ubi is omitted, the program runs the suite:
#   Baseline_NoUBI, UBI_600_Balanced, UBI_1000_Balanced, UBI_1200_Balanced,
#   UBI_1500_DeficitOK, UBI_800_Surplus
```

### Command-Line Options

| Flag           | Type    | Default    | Description                                                                               |
| -------------- | ------- | ---------- | ----------------------------------------------------------------------------------------- |
| `--ubi`        | decimal | *(unset)*  | Monthly UBI per adult. If set → **single scenario**. If omitted → **scenario suite**.     |
| `--months`     | int     | `12`       | Number of simulated months.                                                               |
| `--target-net` | decimal | `0`        | Annualized net fiscal target (e.g., `-200000000000` for deficit tolerance).               |
| `--stochastic` | bool    | `true`     | Enables stochastic sampling (binomial draws). Set `false` for deterministic expectations. |
| `--csv`        | string  | *(unset)*  | Path to write monthly CSV for single scenario.                                            |
| `--summary`    | string  | *(unused)* | Reserved for future extended summaries.                                                   |
| `--seed`       | int     | `42`       | RNG seed for reproducibility.                                                             |
| `--advanced`   | bool    | `true`     | Enables advanced behaviors (richer labor, firm, trade, finance, migration dynamics).      |

*Examples (C# invocation snippet)*

```csharp
// Equivalent to: dotnet run -- --ubi 800 --months 24 --target-net 0 --stochastic false
var args = new[] { "--ubi", "800", "--months", "24", "--target-net", "0", "--stochastic", "false" };
EnhancedUbiSim.EnhancedProgram.Main(args);
```

---

## Conceptual Model

The simulator composes several realistic sub-models with conservative, bounded responses:

* **Tax System**: Progressive PIT brackets (scalable), VAT, corporate tax, capital gains, property, wealth.
* **Population Cohorts**: Income, savings, participation, education, mobility, spend propensities, tax avoidance.
* **Business Ecosystem**: Small/Medium/Large firm buckets with counts, revenues, margins, investment behavior, productivity drift.
* **Labor Market**: Baseline unemployment, mismatch, wage stickiness, union coverage; UBI-linked work incentives.
* **Financial Sector**: Base interest rates, credit spreads/capacity, asset prices; Taylor-style policy rule.
* **Trade**: Export/import elasticities vs. competitiveness (price level & FX), foreign demand growth.
* **Regions**: 4 macro-regions with cost of living, productivity, unemployment differentials.
* **Government**: Baseline spending, social programs, debt path, automatic stabilizers.
* **Migration**: Emigration/immigration sensitive to tax pressure, UBI attraction, unemployment; network effects; re-entry.

### Timeline per Month

1. **Labor update** → unemployment, wage growth, labor supply (Phillips-style effects; UBI & tax influences).
2. **Firms update** → revenues, profits, corporate tax, reinvestment, productivity drift.
3. **Households** → disposable income, consumption (different MPCs for earned vs UBI), VAT, cap-gains, property, wealth taxes; UBI outlays.
4. **Aggregation** → GDP (C+I+G+NX), output gap, price level/inflation (tight bounds), capacity.
5. **Finance** → interest rate via Taylor rule, asset prices, credit growth.
6. **Trade** → exports, imports, exchange rate adjustment.
7. **Migration** → cohort populations, emigration/immigration counts.
8. **Government** → total revenue/outlays, net, debt stock.

**Stability Guardrails** (illustrative):

* Price level clamped to `[0.8, 1.5]`; monthly inflation bounded roughly ±0.5%.
* Nominal GDP bounded `[1T, 4T]` per month for realism and numeric safety.
* Corporate profits, taxes, migration probabilities all bounded to avoid runaway paths.

---

## Tax Calibration

When you specify a fiscal target (`--target-net`), the model:

1. **Coarse grid + fine search** on **corporate tax rate**.
2. If gap remains large, searches **PIT scale** with a **stability bias** toward 1.0.
3. Optionally searches **VAT** within a configurable band.
4. Stops on convergence (gap ≤ tolerance), stagnation, or iteration limits.
5. Returns the calibrated `TaxPolicy` used for the scenario.

Console logs show **rounds**, **final gap**, and final **Corp/PIT/VAT**.

---

## Data Structures (C#)

Key `record` types (partial list):

```csharp
public record TaxPolicy(List<TaxBracket> Brackets, decimal VatRate, decimal CorpTaxRate,
                        decimal CapitalGainsRate, decimal PropertyTaxRate, decimal WealthTaxRate);

public record PopulationCohort(string Name, decimal AvgAnnualIncome, decimal SavingsRate, int Adults,
                               decimal WorkParticipationRate, decimal UbiSpendPropensity, decimal TaxAvoidanceFactor,
                               decimal GeographicMobility, decimal AgeMedian, decimal EducationLevel, string RegionPrimary);

public class EnhancedMonthResult
{
    public int MonthIndex { get; set; }
    public decimal PriceLevel { get; set; }
    public decimal NominalGDP { get; set; }
    public decimal RealGDP { get; set; }
    public decimal UnemploymentRate { get; set; }
    public decimal InterestRate { get; set; }
    public decimal ExchangeRate { get; set; }
    // … government, business, labor, trade, regional, finance fields …
}
```

---

## CSV Outputs

### Monthly CSV (one row per month)

Columns include (subset):

* **Macro**: `Month, PriceLevel, NominalGDP, RealGDP, UnemploymentRate, InterestRate, ExchangeRate`
* **Taxes**: `PersonalIncomeTax, VAT, CorpTax, CapitalGainsTax, PropertyTax, TotalTaxes`
* **Government**: `UBIOutlays, OtherGovSpending, GovernmentDebt, Net`
* **Population**: `Emigrants, Immigrants, RemainingTaxpayers, AvgETR`
* **Firms**: `SmallFirms, MediumFirms, LargeFirms, TotalInvestment, CorporateProfit, TotalCapacity`
* **Labor**: `AvgWage, LaborForceParticipation`
* **Trade**: `Exports, Imports, TradeBalance`
* **Finance/HH**: `TotalSavings, TotalConsumption, AssetPriceIndex, CreditGrowth`

> `Net = TotalTaxes - UBIOutlays - OtherGovSpending` (per month).

### Scenario Summary CSV

* Scenario name, months, UBI level, **NetFiscalPosition** (sum of monthly Nets), **PITScale/CorpRate/VATRate**, totals for **Taxes/UBI/Other**, cumulative **Emigrants/Immigrants**, **FinalTaxpayers**, average **Unemployment**, final **PriceLevel/InterestRate**, final **Firms**, average **TradeBalance**.

---

## Reproducibility

* Set `--seed` and `--stochastic false` to remove randomness entirely (binomial expectations).
* Or keep `--stochastic true` + a fixed seed for repeatable random draws.

---

## Extending the Simulator

* **Add new taxes**: Extend `TaxPolicy`, compute within `CalculateIndividualResponses` and/or firm updates, add to `TotalTaxes`.
* **Change firm buckets**: Update `BusinessEecosystem.FirmSizes` and per-category rules (margins, investment).
* **Richer regions**: Expand `RegionalEconomy.Regions` and modify `UpdateRegionalEconomics`.
* **Alternative policy levers**: E.g., means-testing via `UbiProgram` (`MeansTestingEnabled`, `PhaseOutThreshold`, `PhaseOutRate`).

---

## Design Choices & Limitations

* **Not a forecasting tool**: It’s a **scenario sandbox** with plausible responses and guardrails.
* **Clamps are intentional**: They prevent runaway arithmetic and keep scenarios analyzable.
* **Stylized agents**: Cohorts and firms are grouped for tractability; micro heterogeneity is aggregated.

---

## Troubleshooting

* **Weird numbers?** Make sure `--advanced true` if you want richer dynamics; check that your `--months` isn’t too small for convergence.
* **No convergence in calibration**: The console will warn you. Consider loosening the target, increasing months, or allowing VAT to move.
* **CSV looks huge**: That’s expected for longer runs; filter by columns of interest in your analysis notebook/tooling.

---

## Example Analysis (C# quick parse)

```csharp
using System.Globalization;
var lines = System.IO.File.ReadAllLines("out/monthly_ubi1000.csv").Skip(1);
var avgUnemp = lines.Select(l => decimal.Parse(l.Split(',')[4], CultureInfo.InvariantCulture)).Average();
Console.WriteLine($"Average monthly unemployment: {avgUnemp:P2}");
```

---

## Project Layout

* Single file/program for portability (namespace `EnhancedUbiSim`).
* Entry point: `EnhancedProgram.Main(args)`
* Major sections: **Data Structures**, **Results**, **Calibration**, **Simulation Engine**, **IO/CSV**, **CLI Parsing**.

---

## Contributing

Issues and PRs welcome! Ideas:

* Alternative **Phillips curve** or **Taylor rule** formulations.
* More granular **industry** or **cohort** segmentation.
* Deeper **migration** modeling (visa frictions, wage differentials, amenity shocks).
* Visualization scripts (JS or C#) and notebooks for post-processing.

---

## FAQ

**Q: Can I run without UBI to get a baseline?**
Yes—omit `--ubi` to run the scenario suite which includes `Baseline_NoUBI`.

**Q: How “real” are the numbers?**
They’re **stylized and bounded**. Calibrations target internal consistency and comparative insights, not point predictions.

**Q: Does means-testing work?**
The model supports it (`UbiProgram.MeansTestingEnabled`, `PhaseOutThreshold/Rate`), but the scenario runner keeps it off by default to isolate pure UBI effects.

**Q: How do I hit a surplus/deficit?**
Use `--target-net` with positive (surplus) or negative (deficit) annualized goals; calibration adjusts Corp/PIT/VAT within bounds.

---

<img width="3515" height="315" alt="image" src="https://github.com/user-attachments/assets/33936ee7-7573-4515-863f-89545ac7a6de" />

