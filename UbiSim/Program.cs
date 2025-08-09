using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EnhancedUbiSim
{
    // ==========================================
    // ENHANCED DATA STRUCTURES FOR REALISM
    // ==========================================
    
    public record TaxBracket(decimal Threshold, decimal Rate);
    public record TaxPolicy(
        List<TaxBracket> Brackets,
        decimal VatRate,
        decimal CorpTaxRate,
        decimal CapitalGainsRate,      
        decimal PropertyTaxRate,
        decimal WealthTaxRate
    );

    public record PopulationCohort(
        string Name,
        decimal AvgAnnualIncome,
        decimal SavingsRate,
        int Adults,
        decimal WorkParticipationRate,  
        decimal UbiSpendPropensity,     
        decimal TaxAvoidanceFactor,     
        decimal GeographicMobility,     
        decimal AgeMedian,              
        decimal EducationLevel,         
        string RegionPrimary           
    );

    public record FirmSize(
        string Category,               
        int Count,
        decimal AvgEmployees,
        decimal AvgRevenue,
        decimal EntryBarrier,          
        decimal ExitSensitivity,       
        decimal ProductivityGrowth,
        decimal WageFlexibility,       
        decimal InnovationCapacity     
    );

    public record BusinessEcosystem(
        Dictionary<string, FirmSize> FirmSizes,
        decimal NetworkEffects,        
        decimal InnovationSpillover,   
        decimal GlobalCompetition,     
        decimal SupplyChainIntegration 
    );

    public record LaborMarket(
        decimal UnemploymentRate,
        decimal JobSearchFriction,     
        decimal SkillMismatchFactor,   
        decimal WageStickinessDown,    
        decimal WageStickinessUp,      
        decimal UnionCoverage,         
        Dictionary<string, decimal> RegionalWageGap 
    );

    public record FinancialSector(
        decimal BaseInterestRate,
        decimal CreditSpread,          
        decimal CreditElasticity,      
        decimal SavingsElasticity,     
        decimal AssetPriceLevel,       
        decimal BankLendingCapacity,   
        decimal FinancialStabilityIndex 
    );

    public record TradeBalance(
        decimal ExportLevel,
        decimal ImportLevel,
        decimal ExportElasticity,      
        decimal ImportElasticity,      
        decimal ExchangeRate,          
        decimal ForeignDemandGrowth,   
        decimal TradeAgreementEffect   
    );

    public record RegionalEconomy(
        Dictionary<string, RegionalData> Regions,
        decimal LaborMobility,         
        decimal CapitalMobility,       
        decimal RegionalMultiplier,    
        decimal TransportationCosts    
    );

    public record RegionalData(
        string Name,
        decimal PopulationShare,
        decimal ProductivityLevel,
        decimal CostOfLiving,
        decimal UnemploymentRate,
        decimal HousingCosts,
        decimal LocalTaxRate,          
        decimal AmenityValue          
    );

    public record GovernmentSector(
        decimal BaselineSpending,      
        decimal UnemploymentBenefitRate, 
        decimal SocialSecurityLevel,   
        decimal PublicInvestmentRate,  
        decimal DebtToGDPRatio,       
        decimal AutomaticStabilizerStrength 
    );

    public record UbiProgram(
        decimal MonthlyUbiPerAdult,
        decimal ChildBonus,            
        bool MeansTestingEnabled,      
        decimal PhaseOutThreshold,     
        decimal PhaseOutRate          
    );

    public record EconomyParams(
        decimal BaselineMonthlyGDP,
        decimal PriceAdjustment,
        decimal WageResponse,
        decimal ProductivityGrowth,    
        decimal InflationPersistence, 
        decimal ExpectationAdaptation, 
        decimal MonetaryPolicyRule    
    );

    public record EmigrationParams(
        decimal BaseMonthlyProbByIncome, 
        decimal TaxSensitivity,          
        decimal UbiAttraction,           
        decimal QualityOfLifeEffect,     
        decimal NetworkEffects,          
        decimal ReentryProbability      
    );

    // ==========================================
    // ENHANCED RESULT TRACKING
    // ==========================================

    public class EnhancedMonthResult
    {
        public int MonthIndex { get; set; }
        
        // Macroeconomic indicators
        public decimal PriceLevel { get; set; }
        public decimal NominalGDP { get; set; }
        public decimal RealGDP { get; set; }
        public decimal UnemploymentRate { get; set; }
        public decimal InterestRate { get; set; }
        public decimal ExchangeRate { get; set; }
        
        // Government finances
        public decimal PersonalIncomeTax { get; set; }
        public decimal VatRevenue { get; set; }
        public decimal CorpTax { get; set; }
        public decimal CapitalGainsTax { get; set; }
        public decimal PropertyTax { get; set; }
        public decimal TotalTaxes => PersonalIncomeTax + VatRevenue + CorpTax + CapitalGainsTax + PropertyTax;
        public decimal UbiOutlays { get; set; }
        public decimal OtherGovernmentSpending { get; set; }
        public decimal GovernmentDebt { get; set; }
        
        // Population dynamics
        public int EmigrantsThisMonth { get; set; }
        public int ImmigrantsThisMonth { get; set; }
        public int RemainingTaxpayers { get; set; }
        public decimal AvgEffectiveTaxRate { get; set; }
        
        // Business sector
        public Dictionary<string, int> FirmsBySize { get; set; } = new();
        public Dictionary<string, decimal> ProductivityBySize { get; set; } = new();
        public decimal TotalInvestment { get; set; }
        public decimal CorporateProfit { get; set; }
        public decimal TotalCapacity { get; set; }
        
        // Labor market
        public decimal AvgWageLevel { get; set; }
        public decimal LaborForceParticipation { get; set; }
        public decimal JobVacancyRate { get; set; }
        
        // International
        public decimal ExportsValue { get; set; }
        public decimal ImportsValue { get; set; }
        public decimal TradeBalance => ExportsValue - ImportsValue;
        
        // Regional
        public Dictionary<string, decimal> RegionalUnemployment { get; set; } = new();
        public Dictionary<string, decimal> RegionalMigration { get; set; } = new();
        
        // Financial
        public decimal TotalSavings { get; set; }
        public decimal TotalConsumption { get; set; }
        public decimal AssetPriceIndex { get; set; }
        public decimal CreditGrowth { get; set; }
    }

    // ==========================================
    // MAIN SIMULATION CLASS
    // ==========================================

    class EnhancedProgram
    {
        static bool stochastic = false;
        static Random Rng = new Random(42);
        const decimal CONVERGENCE_TOLERANCE = 100_000_000m; 
        const int MAX_CALIBRATION_ROUNDS = 15;

        static void Main(string[] args)
        {
            // Parse command line arguments
            decimal cliUbi = GetArg(args, "--ubi", decimal.MinValue);
            int months = (int)GetArg(args, "--months", 12m);
            decimal targetAnnualNet = GetArg(args, "--target-net", 0m);
            stochastic = GetArgBool(args, "--stochastic", true);
            string? monthlyCsv = GetArgStr(args, "--csv");
            string? summaryCsv = GetArgStr(args, "--summary");
            int rngSeed = (int)GetArg(args, "--seed", 42m);
            bool advancedMode = GetArgBool(args, "--advanced", true);
            
            Rng = new Random(rngSeed);

            // Initialize enhanced economy
            var (cohorts, labor, business, financial, trade, regional, government, econ, emig) = 
                BuildEnhancedEconomy();

            var baseTax = new TaxPolicy(
                Brackets: new List<TaxBracket>
                {
                    new TaxBracket(0m,      0.00m),
                    new TaxBracket(15_000m, 0.08m),
                    new TaxBracket(40_000m, 0.18m),
                    new TaxBracket(85_000m, 0.28m),
                    new TaxBracket(180_000m,0.35m),
                    new TaxBracket(400_000m,0.39m)
                },
                VatRate: 0.05m,
                CorpTaxRate: 0.25m,
                CapitalGainsRate: 0.20m,
                PropertyTaxRate: 0.015m,
                WealthTaxRate: 0.01m
            );

            if (cliUbi != decimal.MinValue)
            {
                // Single scenario run
                RunSingleScenario(cliUbi, months, targetAnnualNet, stochastic, monthlyCsv, summaryCsv,
                    cohorts, labor, business, financial, trade, regional, government, econ, emig, baseTax, advancedMode);
            }
            else
            {
                // Multiple scenario suite
                RunScenarioSuite(months, stochastic, cohorts, labor, business, financial, trade, regional, 
                    government, econ, emig, baseTax, advancedMode);
            }
        }

        // ==========================================
        // SCENARIO EXECUTION
        // ==========================================

        static void RunSingleScenario(
            decimal ubiAmount, int months, decimal targetNet, bool stochastic,
            string? monthlyCsv, string? summaryCsv,
            List<PopulationCohort> cohorts, LaborMarket labor, BusinessEcosystem business,
            FinancialSector financial, TradeBalance trade, RegionalEconomy regional,
            GovernmentSector government, EconomyParams econ, EmigrationParams emig,
            TaxPolicy baseTax, bool advancedMode)
        {
            var ubi = new UbiProgram(ubiAmount, 200m, false, 0m, 0m);

            Console.WriteLine($"\n=== Enhanced UBI Simulation ===");
            Console.WriteLine($"UBI: ${ubi.MonthlyUbiPerAdult}/month, Target Net: ${targetNet:N0}");
            Console.WriteLine($"Advanced Mode: {advancedMode}, Stochastic: {stochastic}");

            var (calibratedTax, convergenceInfo) = CalibrateEnhancedTaxes(
                months, cohorts, ubi, baseTax, labor, business, financial, trade,
                regional, government, econ, emig, targetNet, stochastic, advancedMode);

            Console.WriteLine($"\n[Calibration Results]");
            Console.WriteLine($"Converged in {convergenceInfo.rounds} rounds");
            Console.WriteLine($"Final gap: ${convergenceInfo.finalGap:N0}");
            Console.WriteLine($"PIT Scale: {convergenceInfo.pitScale:F3}");
            Console.WriteLine($"Corp Rate: {calibratedTax.CorpTaxRate:P1}");
            Console.WriteLine($"VAT Rate: {calibratedTax.VatRate:P1}");

            var results = RunEnhancedSimulation(months, cohorts, ubi, calibratedTax, labor,
                business, financial, trade, regional, government, econ, emig, stochastic, advancedMode);

            PrintEnhancedSummary(results);

            if (!string.IsNullOrWhiteSpace(monthlyCsv))
            {
                WriteEnhancedMonthlyCsv(results, monthlyCsv);
                Console.WriteLine($"\nMonthly CSV written to: {monthlyCsv}");
            }
        }

        static void RunScenarioSuite(
            int months, bool stochastic,
            List<PopulationCohort> baseCohorts, LaborMarket labor, BusinessEcosystem business,
            FinancialSector financial, TradeBalance trade, RegionalEconomy regional,
            GovernmentSector government, EconomyParams econ, EmigrationParams emig,
            TaxPolicy baseTax, bool advancedMode)
        {
            var scenarios = new[]
            {
                new { Name = "Baseline_NoUBI", Ubi = 0m, Target = 0m },
                new { Name = "UBI_600_Balanced", Ubi = 600m, Target = 0m },
                new { Name = "UBI_1000_Balanced", Ubi = 1000m, Target = 0m },
                new { Name = "UBI_1200_Balanced", Ubi = 1200m, Target = 0m },
                new { Name = "UBI_1500_DeficitOK", Ubi = 1500m, Target = -200_000_000_000m },
                new { Name = "UBI_800_Surplus", Ubi = 800m, Target = 100_000_000_000m }
            };

            Directory.CreateDirectory("enhanced_out");
            var summaries = new List<ScenarioSummary>();

            foreach (var scenario in scenarios)
            {
                Console.WriteLine($"\n{'='*50}");
                Console.WriteLine($"Running: {scenario.Name}");
                Console.WriteLine($"{'='*50}");

                Rng = new Random(42); // Reset for consistency

                var cohorts = CloneCohorts(baseCohorts);
                var ubi = new UbiProgram(scenario.Ubi, 200m, false, 0m, 0m);

                var (calibratedTax, convergenceInfo) = CalibrateEnhancedTaxes(
                    months, cohorts, ubi, baseTax, labor, business, financial, trade,
                    regional, government, econ, emig, scenario.Target, stochastic, advancedMode);

                var results = RunEnhancedSimulation(months, cohorts, ubi, calibratedTax, labor,
                    business, financial, trade, regional, government, econ, emig, stochastic, advancedMode);

                PrintEnhancedSummary(results);

                var monthlyPath = Path.Combine("enhanced_out", $"{scenario.Name}_monthly.csv");
                WriteEnhancedMonthlyCsv(results, monthlyPath);

                var summary = SummarizeEnhancedScenario(scenario.Name, months, ubi, calibratedTax, 
                    results, convergenceInfo);
                summaries.Add(summary);
            }

            var summaryPath = Path.Combine("enhanced_out", "enhanced_scenario_summary.csv");
            WriteEnhancedSummaryCsv(summaries, summaryPath);
            Console.WriteLine($"\nAll results written to enhanced_out/ directory");
        }

        // ==========================================
        // ENHANCED TAX CALIBRATION WITH STABILITY
        // ==========================================

        public record ConvergenceInfo(int rounds, decimal finalGap, decimal pitScale, bool converged);

        static (TaxPolicy calibratedTax, ConvergenceInfo info) CalibrateEnhancedTaxes(
            int months,
            List<PopulationCohort> cohorts,
            UbiProgram ubi,
            TaxPolicy baseTax,
            LaborMarket labor,
            BusinessEcosystem business,
            FinancialSector financial,
            TradeBalance trade,
            RegionalEconomy regional,
            GovernmentSector government,
            EconomyParams econ,
            EmigrationParams emig,
            decimal targetNet,
            bool stochastic,
            bool advancedMode)
        {
            decimal pitScale = 1.00m;
            decimal corpRate = baseTax.CorpTaxRate;
            decimal vatRate = baseTax.VatRate;
            
            decimal lastGap = decimal.MaxValue;
            int stagnantRounds = 0;
            bool converged = false;
            int roundsCompleted = 1;

            Console.WriteLine($"\nCalibrating for UBI=${ubi.MonthlyUbiPerAdult}, Target=${targetNet:N0}");
            Console.WriteLine($"Convergence tolerance: ${CONVERGENCE_TOLERANCE:N0}");

            for (int round = 0; round < MAX_CALIBRATION_ROUNDS; round++)
            {
                roundsCompleted = round + 1;
                Console.WriteLine($"\n--- Calibration Round {roundsCompleted}/{MAX_CALIBRATION_ROUNDS} ---");
                
                try
                {
                    // PHASE 1: Optimize corporate rate (main revenue lever)
                    Console.WriteLine("Optimizing corporate tax rate...");
                    corpRate = SearchOptimalCorpRate(months, cohorts, ubi, baseTax, pitScale, vatRate,
                        labor, business, financial, trade, regional, government, econ, emig,
                        targetNet, stochastic, advancedMode, 0.10m, 0.35m);

                    // PHASE 2: Test and potentially adjust other rates
                    var testTax = CreateCalibratedTax(baseTax, pitScale, corpRate, vatRate);
                    var testResults = RunEnhancedSimulation(months, CloneCohorts(cohorts), ubi, testTax,
                        labor, business, financial, trade, regional, government, econ, emig, stochastic, advancedMode);

                    var actualNet = CalculateNetFiscalPosition(testResults);
                    var gap = Math.Abs(actualNet - targetNet);

                    Console.WriteLine($"Round {roundsCompleted} Result: Gap=${gap:N0}, Corp={corpRate:P1}, PIT={pitScale:F3}, VAT={vatRate:P1}");
                    Console.WriteLine($"  Actual Net: ${actualNet:N0}, Target: ${targetNet:N0}");

                    // Check for convergence
                    if (gap <= CONVERGENCE_TOLERANCE)
                    {
                        converged = true;
                        Console.WriteLine($"*** CALIBRATION CONVERGED in {roundsCompleted} rounds! ***");
                        break;
                    }

                    // PHASE 3: If gap is still large, optimize other instruments
                    if (gap > CONVERGENCE_TOLERANCE * 5m && roundsCompleted <= 10) // Only first 10 rounds
                    {
                        Console.WriteLine("Gap large, optimizing PIT scale...");
                        pitScale = SearchPitScaleWithStabilityBias(months, cohorts, ubi, baseTax, corpRate, vatRate,
                            labor, business, financial, trade, regional, government, econ, emig,
                            targetNet, stochastic, advancedMode, 0.85m, 1.15m);
                        
                        // Re-test with new PIT scale
                        testTax = CreateCalibratedTax(baseTax, pitScale, corpRate, vatRate);
                        testResults = RunEnhancedSimulation(months, CloneCohorts(cohorts), ubi, testTax,
                            labor, business, financial, trade, regional, government, econ, emig, stochastic, advancedMode);
                        actualNet = CalculateNetFiscalPosition(testResults);
                        gap = Math.Abs(actualNet - targetNet);
                        
                        Console.WriteLine($"After PIT optimization: Gap=${gap:N0}, PIT={pitScale:F3}");

                        if (gap > CONVERGENCE_TOLERANCE * 3m)
                        {
                            Console.WriteLine("Gap still large, optimizing VAT rate...");
                            vatRate = SearchOptimalVatRate(months, cohorts, ubi, baseTax, pitScale, corpRate,
                                labor, business, financial, trade, regional, government, econ, emig,
                                targetNet, stochastic, advancedMode, 0.02m, 0.12m);
                        }
                    }

                    // Check for improvement/stagnation
                    if (Math.Abs(gap - lastGap) < CONVERGENCE_TOLERANCE * 0.1m)
                    {
                        stagnantRounds++;
                        if (stagnantRounds >= 3)
                        {
                            Console.WriteLine("Calibration stagnated, stopping early");
                            break;
                        }
                    }
                    else
                    {
                        stagnantRounds = 0;
                    }

                    lastGap = gap;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR in calibration round {roundsCompleted}: {ex.Message}");
                    break;
                }
            }

            var finalTax = CreateCalibratedTax(baseTax, pitScale, corpRate, vatRate);
            var convergenceInfo = new ConvergenceInfo(roundsCompleted, lastGap, pitScale, converged);

            Console.WriteLine($"\n*** CALIBRATION COMPLETE ***");
            Console.WriteLine($"Rounds: {roundsCompleted}, Converged: {converged}, Final Gap: ${lastGap:N0}");
            Console.WriteLine($"Final Rates: Corp={corpRate:P1}, PIT={pitScale:F3}, VAT={vatRate:P1}");
            
            if (!converged)
            {
                Console.WriteLine($"WARNING: Did not converge to ${CONVERGENCE_TOLERANCE:N0} tolerance");
            }

            return (finalTax, convergenceInfo);
        }

        static TaxPolicy CreateCalibratedTax(TaxPolicy baseTax, decimal pitScale, decimal corpRate, decimal vatRate)
        {
            return baseTax with
            {
                Brackets = baseTax.Brackets.Select(b => 
                    new TaxBracket(b.Threshold, Math.Min(0.65m, b.Rate * pitScale))).ToList(),
                CorpTaxRate = corpRate,
                VatRate = vatRate
            };
        }

        static decimal SearchOptimalCorpRate(
            int months, List<PopulationCohort> cohorts, UbiProgram ubi, TaxPolicy baseTax,
            decimal pitScale, decimal vatRate, LaborMarket labor, BusinessEcosystem business,
            FinancialSector financial, TradeBalance trade, RegionalEconomy regional,
            GovernmentSector government, EconomyParams econ, EmigrationParams emig,
            decimal targetNet, bool stochastic, bool advancedMode, decimal minRate, decimal maxRate)
        {
            //Console.WriteLine($"[DEBUG] Searching corporate rate in range [{minRate:P1}, {maxRate:P1}] for target ${targetNet:N0}");
            
            // FIXED: Much wider and more realistic search range
            var adjustedMinRate = 0.05m; // 5% minimum (very low)
            var adjustedMaxRate = 0.50m; // 50% maximum (very high but possible)
            
            // PHASE 1: Coarse grid search with wide range
            decimal bestRate = adjustedMinRate;
            decimal bestGap = decimal.MaxValue;
            
            // Test 10 points across full range
            for (int i = 0; i <= 10; i++)
            {
                decimal testRate = adjustedMinRate + (adjustedMaxRate - adjustedMinRate) * i / 10m;
                try
                {
                    var tax = CreateCalibratedTax(baseTax, pitScale, testRate, vatRate);
                    var results = RunEnhancedSimulation(months, CloneCohorts(cohorts), ubi, tax,
                        labor, business, financial, trade, regional, government, econ, emig, stochastic, advancedMode);
                    var netResult = CalculateNetFiscalPosition(results);
                    var gap = Math.Abs(netResult - targetNet);
                    
                    //Console.WriteLine($"[DEBUG] Grid test {testRate:P1} -> Net ${netResult:N0}, Gap ${gap:N0}");
                    
                    if (gap < bestGap && Math.Abs(netResult) < 50_000_000_000_000m) // $50T sanity check
                    {
                        bestGap = gap;
                        bestRate = testRate;
                        //Console.WriteLine($"[DEBUG] *** NEW BEST: {testRate:P1} with gap ${gap:N0} ***");
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine($"[DEBUG] Grid test {testRate:P1}: ERROR - {ex.Message}");
                }
            }
            
            // PHASE 2: Fine binary search around best point
            var searchRadius = (adjustedMaxRate - adjustedMinRate) * 0.2m; // 20% of range
            var lo = Math.Max(adjustedMinRate, bestRate - searchRadius);
            var hi = Math.Min(adjustedMaxRate, bestRate + searchRadius);
            
            //Console.WriteLine($"[DEBUG] Fine search around {bestRate:P1} in [{lo:P1}, {hi:P1}]");
            
            for (int i = 0; i < 100; i++) // More iterations
            {
                decimal mid = (lo + hi) / 2m;
                
                if (hi - lo < 0.0001m) break; // Tighter precision
                try
                {
                    var tax = CreateCalibratedTax(baseTax, pitScale, mid, vatRate);
                    var results = RunEnhancedSimulation(months, CloneCohorts(cohorts), ubi, tax,
                        labor, business, financial, trade, regional, government, econ, emig, stochastic, advancedMode);
                    var actualNet = CalculateNetFiscalPosition(results);
                    var gap = Math.Abs(actualNet - targetNet);
                    
                    if (gap < bestGap)
                    {
                        bestGap = gap;
                        bestRate = mid;
                        //Console.WriteLine($"[DEBUG] Binary improvement {mid:P1}: Gap ${gap:N0}");
                    }
                    
                    if (gap <= CONVERGENCE_TOLERANCE)
                    {
                        //Console.WriteLine($"[DEBUG] *** CONVERGED at {mid:P1} ***");
                        return mid;
                    }
                    
                    if (actualNet > targetNet) hi = mid;
                    else lo = mid;
                }
                catch (Exception ex)
                {
                    //Console.WriteLine($"[DEBUG] Binary error at {mid:P1}: {ex.Message}");
                    if (mid > bestRate) hi = (hi + bestRate) / 2m;
                    else lo = (lo + bestRate) / 2m;
                }
                
                if (hi - lo < 0.005m) break; // 0.5% precision
            }
            
            //Console.WriteLine($"[DEBUG] Final corp rate: {bestRate:P1}, Gap ${bestGap:N0}");
            return bestRate;
        }

        static decimal SearchPitScaleWithStabilityBias(
            int months, List<PopulationCohort> cohorts, UbiProgram ubi, TaxPolicy baseTax,
            decimal corpRate, decimal vatRate, LaborMarket labor, BusinessEcosystem business,
            FinancialSector financial, TradeBalance trade, RegionalEconomy regional,
            GovernmentSector government, EconomyParams econ, EmigrationParams emig,
            decimal targetNet, bool stochastic, bool advancedMode, decimal minScale, decimal maxScale)
        {
            // Bias toward keeping PIT scale near 1.0 for political feasibility
            decimal bestScale = 1.0m;
            decimal bestScore = decimal.MaxValue;

            for (int i = 0; i < 20; i++)
            {
                decimal testScale = minScale + (maxScale - minScale) * i / 19m;
                var tax = CreateCalibratedTax(baseTax, testScale, corpRate, vatRate);
                var results = RunEnhancedSimulation(months, CloneCohorts(cohorts), ubi, tax,
                    labor, business, financial, trade, regional, government, econ, emig, stochastic, advancedMode);
                
                var actualNet = CalculateNetFiscalPosition(results);
                var fiscalGap = Math.Abs(actualNet - targetNet);
                
                // Add penalty for moving away from 1.0
                var stabilityPenalty = Math.Pow((double)(testScale - 1.0m), 2) * 1_000_000_000; // $1B per 0.1 squared
                var totalScore = fiscalGap + (decimal)stabilityPenalty;

                if (totalScore < bestScore)
                {
                    bestScore = totalScore;
                    bestScale = testScale;
                }
            }

            return bestScale;
        }

        static decimal SearchOptimalVatRate(
            int months, List<PopulationCohort> cohorts, UbiProgram ubi, TaxPolicy baseTax,
            decimal pitScale, decimal corpRate, LaborMarket labor, BusinessEcosystem business,
            FinancialSector financial, TradeBalance trade, RegionalEconomy regional,
            GovernmentSector government, EconomyParams econ, EmigrationParams emig,
            decimal targetNet, bool stochastic, bool advancedMode, decimal minRate, decimal maxRate)
        {
            return BinarySearchTaxParameter(
                rate => {
                    var tax = CreateCalibratedTax(baseTax, pitScale, corpRate, rate);
                    var results = RunEnhancedSimulation(months, CloneCohorts(cohorts), ubi, tax,
                        labor, business, financial, trade, regional, government, econ, emig, stochastic, advancedMode);
                    return CalculateNetFiscalPosition(results);
                },
                targetNet, minRate, maxRate, 100);
        }

        static decimal BinarySearchTaxParameter(
            Func<decimal, decimal> evaluateNet, decimal targetNet, 
            decimal minValue, decimal maxValue, int maxIterations)
        {
            decimal bestRate = (minValue + maxValue) / 2m;
            decimal bestGap = decimal.MaxValue;
            
            ////Console.WriteLine($"[DEBUG] Starting search in [{minValue:P1}, {maxValue:P1}] for target ${targetNet:N0}");

            // PHASE 1: Grid search to find best starting point
            var gridPoints = 20;
            for (int i = 0; i <= gridPoints; i++)
            {
                decimal testRate = minValue + (maxValue - minValue) * i / gridPoints;
                try
                {
                    decimal testNet = evaluateNet(testRate);
                    decimal testGap = Math.Abs(testNet - targetNet);
                    
                    if (testGap < bestGap && Math.Abs(testNet) < 20_000_000_000_000m) // $20T sanity check
                    {
                        bestGap = testGap;
                        bestRate = testRate;
                        ////Console.WriteLine($"[DEBUG] Grid point {testRate:P1}: Gap=${testGap:N0} (NEW BEST)");
                    }
                    else
                    {
                        ////Console.WriteLine($"[DEBUG] Grid point {testRate:P1}: Gap=${testGap:N0}");
                    }
                }
                catch (Exception ex)
                {
                    ////Console.WriteLine($"[DEBUG] Grid point {testRate:P1}: ERROR - {ex.Message}");
                }
            }

            // PHASE 2: Fine-tune around best point with binary search
            decimal lo = Math.Max(minValue, bestRate - (maxValue - minValue) * 0.1m);
            decimal hi = Math.Min(maxValue, bestRate + (maxValue - minValue) * 0.1m);
            
            ////Console.WriteLine($"[DEBUG] Fine-tuning around {bestRate:P1} in range [{lo:P1}, {hi:P1}]");

            for (int i = 0; i < maxIterations; i++)
            {
                decimal mid = (lo + hi) / 2m;
                
                try
                {
                    decimal actualNet = evaluateNet(mid);
                    
                    if (Math.Abs(actualNet) > 20_000_000_000_000m)
                    {
                        ////Console.WriteLine($"[DEBUG] Unstable at {mid:P1}: ${actualNet:N0}");
                        if (mid > bestRate) hi = (hi + bestRate) / 2m;
                        else lo = (lo + bestRate) / 2m;
                        continue;
                    }
                    
                    decimal gap = Math.Abs(actualNet - targetNet);

                    if (gap < bestGap)
                    {
                        bestGap = gap;
                        bestRate = mid;
                        ////Console.WriteLine($"[DEBUG] Binary improvement {mid:P1}: Gap=${gap:N0} (NEW BEST)");
                    }

                    if (gap <= CONVERGENCE_TOLERANCE) 
                    {
                        ////Console.WriteLine($"[DEBUG] *** CONVERGED at {mid:P1} with gap ${gap:N0} ***");
                        bestRate = mid;
                        break;
                    }

                    if (actualNet > targetNet) hi = mid;
                    else lo = mid;
                }
                catch (Exception ex)
                {
                    ////Console.WriteLine($"[DEBUG] Binary error at {mid:P1}: {ex.Message}");
                    if (mid > bestRate) hi = (hi + bestRate) / 2m;
                    else lo = (lo + bestRate) / 2m;
                }

                if (hi - lo < 0.001m) break; // 0.1% precision
            }

            ////Console.WriteLine($"[DEBUG] Final result: Rate={bestRate:P1}, Gap=${bestGap:N0}");
            return bestRate;
        }


        // ==========================================
        // ENHANCED SIMULATION ENGINE - FIXED
        // ==========================================

        static List<EnhancedMonthResult> RunEnhancedSimulation(
            int months,
            List<PopulationCohort> cohorts,
            UbiProgram ubi,
            TaxPolicy tax,
            LaborMarket labor,
            BusinessEcosystem business,
            FinancialSector financial,
            TradeBalance trade,
            RegionalEconomy regional,
            GovernmentSector government,
            EconomyParams econ,
            EmigrationParams emig,
            bool stochastic,
            bool advancedMode)
        {
            var results = new List<EnhancedMonthResult>();
            
            // Initialize state variables with proper bounds
            decimal priceLevel = 1.00m;
            decimal unemploymentRate = labor.UnemploymentRate;
            decimal interestRate = financial.BaseInterestRate;
            decimal exchangeRate = 1.00m;
            decimal assetPriceIndex = 1.00m;
            decimal governmentDebt = government.DebtToGDPRatio * econ.BaselineMonthlyGDP * 12m;
            decimal cumulativeInflation = 1.0m; // Track cumulative price changes
            
            var currentBusiness = CloneBusinessEcosystem(business);
            var cohortPopulations = cohorts.ToDictionary(c => c.Name, c => c.Adults);
            var regionalState = CloneRegionalEconomy(regional);

            for (int m = 1; m <= months; m++)
            {
                // ===== LABOR MARKET DYNAMICS - FIXED =====
                var (newUnemploymentRate, wageGrowth, laborSupply) = UpdateLaborMarket(
                    cohorts, cohortPopulations, ubi, tax, unemploymentRate, labor, 
                    priceLevel, m, advancedMode);
                unemploymentRate = Clamp(newUnemploymentRate, 0.02m, 0.15m); // Reasonable bounds

                // ===== BUSINESS SECTOR DYNAMICS - FIXED =====
                var (newBusiness, totalInvestment, totalProfit, corporateTax) = UpdateBusinessSector(
                    currentBusiness, unemploymentRate, interestRate, priceLevel, 
                    financial, tax, ubi, stochastic, advancedMode);
                currentBusiness = newBusiness;

                // ===== INDIVIDUAL CONSUMPTION & TAXES - FIXED =====
                var (totalConsumption, taxData, ubiOutlays) = CalculateIndividualResponses(
                    cohorts, cohortPopulations, ubi, tax, wageGrowth, unemploymentRate,
                    assetPriceIndex, priceLevel, m, advancedMode);

                // ===== AGGREGATE DEMAND & SUPPLY - WITH BOUNDS =====
                var netExports = CalculateNetExports(trade, priceLevel, exchangeRate);
                netExports = Clamp(netExports, -200_000_000_000m, 200_000_000_000m); 
                
                var nominalGDP = SafeAdd(totalConsumption, totalInvestment, 5_000_000_000_000m);
                nominalGDP = SafeAdd(nominalGDP, government.BaselineSpending, 5_000_000_000_000m);
                nominalGDP = SafeAdd(nominalGDP, netExports, 5_000_000_000_000m);
                nominalGDP = Clamp(nominalGDP, 1_000_000_000_000m, 4_000_000_000_000m); // Tighter GDP bounds

                var realGDP = nominalGDP / Math.Max(0.8m, priceLevel); // Prevent extreme real GDP
                var totalCapacity = CalculateTotalCapacity(currentBusiness);
                totalCapacity = Clamp(totalCapacity, 0.7m, 1.3m); // Tighter capacity bounds
                
                var baselineCapacityGDP = econ.BaselineMonthlyGDP * totalCapacity;
                baselineCapacityGDP = Math.Max(1m, baselineCapacityGDP); 
                var outputGap = Clamp((realGDP / baselineCapacityGDP) - 1m, -0.3m, 0.3m); // Tighter output gap

                // ===== PRICE DYNAMICS - FIXED TO PREVENT HYPERINFLATION =====
                var inflation = CalculateInflation(outputGap, priceLevel, econ, cumulativeInflation, m);
                inflation = Clamp(inflation, -0.01m, 0.02m); // Much tighter inflation bounds!
                priceLevel = Math.Max(0.8m, priceLevel * (1m + inflation));
                priceLevel = Clamp(priceLevel, 0.8m, 1.5m); // Prevent hyperinflation!
                cumulativeInflation *= (1m + inflation);

                // ===== FINANCIAL MARKET - FIXED TAYLOR RULE =====
                var (newInterestRate, newAssetPrices, creditGrowth) = UpdateFinancialMarkets(
                    interestRate, inflation, unemploymentRate, outputGap, assetPriceIndex, 
                    financial, econ, cumulativeInflation, advancedMode); // Pass cumulative inflation
                interestRate = Clamp(newInterestRate, 0.005m, 0.08m); // Responsive interest rates
                assetPriceIndex = Clamp(newAssetPrices, 0.7m, 1.5m);
                creditGrowth = Clamp(creditGrowth, -0.1m, 0.2m);

                // ===== INTERNATIONAL TRADE =====
                var (exports, imports, newExchangeRate) = UpdateInternationalTrade(
                    trade, priceLevel, nominalGDP, exchangeRate, advancedMode);
                exchangeRate = newExchangeRate;

                // ===== MIGRATION & EMIGRATION - FIXED =====
                var (emigrants, immigrants) = UpdateMigrationFlows(
                    cohorts, cohortPopulations, tax, ubi, unemploymentRate, emig, stochastic, advancedMode);

                // ===== GOVERNMENT BUDGET - WITH CORPORATE TAX =====
                var totalTaxRevenue = taxData.personalIncome + taxData.vat + corporateTax + taxData.capitalGains + taxData.property + taxData.wealth;
                totalTaxRevenue = Clamp(totalTaxRevenue, 0m, 1_000_000_000_000m); 
                
                var netGovernmentPosition = totalTaxRevenue - ubiOutlays - government.BaselineSpending;
                netGovernmentPosition = Clamp(netGovernmentPosition, -500_000_000_000m, 500_000_000_000m); 
                
                governmentDebt -= netGovernmentPosition; 
                governmentDebt = Clamp(governmentDebt, 0m, 30_000_000_000_000m); 

                // ===== REGIONAL DYNAMICS =====
                if (advancedMode)
                {
                    regionalState = UpdateRegionalEconomics(regionalState, unemploymentRate, 
                        cohortPopulations, priceLevel);
                }

                // Record results
                var monthResult = new EnhancedMonthResult
                {
                    MonthIndex = m,
                    PriceLevel = priceLevel,
                    NominalGDP = nominalGDP,
                    RealGDP = realGDP,
                    UnemploymentRate = unemploymentRate,
                    InterestRate = interestRate,
                    ExchangeRate = exchangeRate,
                    PersonalIncomeTax = taxData.personalIncome,
                    VatRevenue = taxData.vat,
                    CorpTax = corporateTax, // Now properly included!
                    CapitalGainsTax = taxData.capitalGains,
                    PropertyTax = taxData.property,
                    UbiOutlays = ubiOutlays,
                    OtherGovernmentSpending = government.BaselineSpending,
                    GovernmentDebt = governmentDebt,
                    EmigrantsThisMonth = emigrants,
                    ImmigrantsThisMonth = immigrants,
                    RemainingTaxpayers = cohortPopulations.Values.Sum(),
                    AvgEffectiveTaxRate = CalculateAvgEffectiveTaxRate(cohorts, cohortPopulations, tax),
                    FirmsBySize = currentBusiness.FirmSizes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count),
                    TotalInvestment = totalInvestment,
                    CorporateProfit = totalProfit,
                    TotalCapacity = totalCapacity,
                    AvgWageLevel = CalculateAvgWageLevel(cohorts, wageGrowth),
                    LaborForceParticipation = CalculateLaborForceParticipation(cohorts, cohortPopulations),
                    ExportsValue = exports,
                    ImportsValue = imports,
                    TotalSavings = CalculateTotalSavings(cohorts, cohortPopulations, totalConsumption),
                    TotalConsumption = totalConsumption,
                    AssetPriceIndex = assetPriceIndex,
                    CreditGrowth = creditGrowth
                };

                if (advancedMode)
                {
                    monthResult.RegionalUnemployment = regionalState.Regions.ToDictionary(
                        kvp => kvp.Key, kvp => kvp.Value.UnemploymentRate);
                }

                results.Add(monthResult);
            }

            return results;
        }

        // ==========================================
        // INDIVIDUAL BEHAVIOR MODELING - UNCHANGED
        // ==========================================

        static (decimal totalConsumption, 
            (decimal personalIncome, decimal vat, decimal capitalGains, decimal property, decimal wealth)
            totalTaxes,
                decimal ubiOutlays) CalculateIndividualResponses(
            List<PopulationCohort> cohorts,
            Dictionary<string, int> populations,
            UbiProgram ubi,
            TaxPolicy tax,
            decimal wageGrowth,
            decimal unemploymentRate,
            decimal assetPriceIndex,
            decimal priceLevel,
            int month,
            bool advancedMode)
        {
            decimal totalConsumption = 0m;
            decimal totalWealthTax = 0m;
            decimal totalPersonalIncomeTax = 0m;
            decimal totalVat = 0m;
            decimal totalUbiOutlays = 0m;
            decimal totalCapitalGains = 0m;
            decimal totalProperty = 0m;

            const decimal MAX_INCOME = 50_000_000m; 
            const decimal MAX_CONSUMPTION = 5_000_000m; 
            const decimal MAX_TAX = 25_000_000m; 
            const decimal MAX_TOTAL_VALUE = 500_000_000_000_000m; 

            foreach (var cohort in cohorts)
            {
                var population = populations[cohort.Name];
                if (population <= 0) continue;

                // Employment and income with safety bounds
                var employmentRate = Math.Max(0.5m, 1m - unemploymentRate * (2m - cohort.EducationLevel));
                var activeWorkers = (int)(population * cohort.WorkParticipationRate * employmentRate);
                
                // Calculate work disincentive from UBI with bounds
                var workDisincentive = CalculateWorkDisincentive(cohort, ubi, tax, advancedMode);
                workDisincentive = Clamp(workDisincentive, 0m, 0.30m); // Max 30% work reduction (more realistic)
                
                var adjustedIncome = cohort.AvgAnnualIncome * (1m + Clamp(wageGrowth, -0.2m, 0.5m)) * (1m - workDisincentive);
                adjustedIncome = Clamp(adjustedIncome, 5000m, MAX_INCOME); 
                
                var monthlyEarnedIncome = adjustedIncome / 12m;

                // UBI calculation 
                var monthlyUbi = CalculateUbiPayment(cohort, ubi, adjustedIncome);
                monthlyUbi = Clamp(monthlyUbi, 0m, 10_000m); 
                
                // Tax calculations with bounds
                var personalIncomeTax = CalculatePersonalIncomeTax(adjustedIncome, tax.Brackets) / 12m;
                personalIncomeTax = Clamp(personalIncomeTax, 0m, MAX_TAX);
                
                // After-tax income and consumption
                var afterTaxIncome = monthlyEarnedIncome - personalIncomeTax + monthlyUbi;
                afterTaxIncome = Math.Max(0m, afterTaxIncome);
                
                // Enhanced consumption model with bounds
                var consumption = CalculateEnhancedConsumption(
                    cohort, afterTaxIncome, monthlyEarnedIncome, monthlyUbi, 
                    priceLevel, assetPriceIndex, unemploymentRate, advancedMode);
                consumption = Clamp(consumption, 0m, MAX_CONSUMPTION);

                // VAT on consumption
                var vat = consumption * Clamp(tax.VatRate, 0m, 0.3m); 
                vat = Clamp(vat, 0m, MAX_TAX);

                // Capital gains and property taxes 
                var capitalGains = CalculateCapitalGainsTax(cohort, assetPriceIndex, tax.CapitalGainsRate, month);
                capitalGains = Clamp(capitalGains, 0m, MAX_TAX);
                
                var propertyTax = CalculatePropertyTax(cohort, tax.PropertyTaxRate);
                propertyTax = Clamp(propertyTax, 0m, MAX_TAX);
                
                // Wealth Tax: Only applied to highest cohorts
                var wealthTax = (cohort.AvgAnnualIncome > 1_000_000m) ?
                    ((adjustedIncome - 1_000_000m) * tax.WealthTaxRate / 12m) : 0m;
                wealthTax = Clamp(wealthTax, 0m, MAX_TAX);

                // Multiply by population, add to total
                var wealthContrib = SafeMultiply(wealthTax, population, MAX_TOTAL_VALUE);
                totalWealthTax = SafeAdd(totalWealthTax, wealthContrib, MAX_TOTAL_VALUE);

                // Aggregate with overflow protection
                var consumptionContrib = SafeMultiply(consumption, activeWorkers, MAX_TOTAL_VALUE);
                var pitContrib = SafeMultiply(personalIncomeTax, activeWorkers, MAX_TOTAL_VALUE);
                var vatContrib = SafeMultiply(vat, activeWorkers, MAX_TOTAL_VALUE);
                var ubiContrib = SafeMultiply(monthlyUbi, population, MAX_TOTAL_VALUE);
                var cgContrib = SafeMultiply(capitalGains, population, MAX_TOTAL_VALUE);
                var propContrib = SafeMultiply(propertyTax, population, MAX_TOTAL_VALUE);

                totalConsumption = SafeAdd(totalConsumption, consumptionContrib, MAX_TOTAL_VALUE);
                totalPersonalIncomeTax = SafeAdd(totalPersonalIncomeTax, pitContrib, MAX_TOTAL_VALUE);
                totalVat = SafeAdd(totalVat, vatContrib, MAX_TOTAL_VALUE);
                totalUbiOutlays = SafeAdd(totalUbiOutlays, ubiContrib, MAX_TOTAL_VALUE);
                totalCapitalGains = SafeAdd(totalCapitalGains, cgContrib, MAX_TOTAL_VALUE);
                totalProperty = SafeAdd(totalProperty, propContrib, MAX_TOTAL_VALUE);
            }

            return (totalConsumption, 
                (totalPersonalIncomeTax, totalVat, totalCapitalGains, totalProperty, totalWealthTax),
                totalUbiOutlays);
        }

        // Safe arithmetic operations to prevent overflow
        static decimal SafeMultiply(decimal a, decimal b, decimal maxValue)
        {
            if (a == 0m || b == 0m) return 0m;
            if (Math.Abs(a) > maxValue / Math.Abs(b)) return Math.Sign(a) * Math.Sign(b) * maxValue;
            var result = a * b;
            return Math.Min(Math.Abs(result), maxValue) * Math.Sign(result);
        }

        static decimal SafeAdd(decimal a, decimal b, decimal maxValue)
        {
            if (a > maxValue - Math.Max(0, b)) return maxValue;
            if (b > maxValue - Math.Max(0, a)) return maxValue;
            return Math.Min(a + b, maxValue);
        }

        static decimal CalculateWorkDisincentive(PopulationCohort cohort, UbiProgram ubi, TaxPolicy tax, bool advancedMode)
        {
            if (!advancedMode) return 0m;

            var monthlyIncome = Math.Max(1000m, cohort.AvgAnnualIncome / 12m); 
            var ubiAmount = Math.Min(3000m, ubi.MonthlyUbiPerAdult); // More realistic max
            
            // UBI creates work disincentive, but not extreme
            var incomeRatio = ubiAmount / monthlyIncome;
            var baseDisincentive = Math.Min(0.10m, incomeRatio * 0.15m); // Smaller effect
            
            // Adjust for individual characteristics (smaller effects)
            if (cohort.AgeMedian > 55m) baseDisincentive *= 1.2m; 
            if (cohort.EducationLevel < 0.5m) baseDisincentive *= 1.1m; 
            
            // Tax system interaction (smaller effect)
            var effectiveTaxRate = CalculateEffectiveTaxRate(cohort.AvgAnnualIncome, tax.Brackets);
            effectiveTaxRate = Clamp(effectiveTaxRate, 0m, 0.6m); 
            var taxDisincentive = effectiveTaxRate * 0.05m; // Smaller tax response
            
            return Clamp(baseDisincentive + taxDisincentive, 0m, 0.25m); // Max 25% work reduction
        }

        static decimal CalculateUbiPayment(PopulationCohort cohort, UbiProgram ubi, decimal annualIncome)
        {
            var baseUbi = ubi.MonthlyUbiPerAdult;
            
            if (!ubi.MeansTestingEnabled) return baseUbi;
            
            // Phase out UBI for high earners
            if (annualIncome <= ubi.PhaseOutThreshold) return baseUbi;
            
            var phaseOutAmount = (annualIncome - ubi.PhaseOutThreshold) * ubi.PhaseOutRate / 12m;
            return Math.Max(0m, baseUbi - phaseOutAmount);
        }

        static decimal CalculateEnhancedConsumption(
            PopulationCohort cohort, decimal afterTaxIncome, decimal earnedIncome, decimal ubiIncome,
            decimal priceLevel, decimal assetPriceIndex, decimal unemploymentRate, bool advancedMode)
        {
            if (!advancedMode)
            {
                // Simple model: standard MPC
                return Math.Max(0m, afterTaxIncome * (1m - cohort.SavingsRate));
            }

            // Enhanced model with different MPCs for different income sources
            var earnedMPC = 1m - cohort.SavingsRate;
            var ubiMPC = Math.Min(1.0m, cohort.UbiSpendPropensity); // Cap at 100%
            
            var baseConsumption = earnedIncome * earnedMPC + ubiIncome * ubiMPC;
            
            // Wealth effects from asset prices (smaller effect)
            var wealthEffect = (assetPriceIndex - 1m) * 0.01m * cohort.AvgAnnualIncome / 12m; // Smaller effect
            if (cohort.AvgAnnualIncome > 100_000m) wealthEffect *= 1.5m; 
            
            // Precautionary savings during high unemployment (smaller effect)
            var precautionaryAdjustment = -unemploymentRate * 0.2m * afterTaxIncome; // Smaller effect
            
            // Price level effects (smaller elasticity)
            var priceElasticity = cohort.AvgAnnualIncome < 50_000m ? -0.1m : -0.2m; // Smaller elasticity
            var priceAdjustment = 1m + priceElasticity * (priceLevel - 1m);
            
            var adjustedConsumption = (baseConsumption + wealthEffect + precautionaryAdjustment) * priceAdjustment;
            
            return Math.Max(afterTaxIncome * 0.3m, adjustedConsumption); // Minimum consumption floor
        }

        static decimal CalculateCapitalGainsTax(PopulationCohort cohort, decimal assetPriceIndex, 
            decimal taxRate, int month)
        {
            // Only higher-income cohorts have significant capital gains
            if (cohort.AvgAnnualIncome < 75_000m) return 0m;
            
            // Assume asset appreciation translates to realized gains 
            var assetHoldings = (cohort.AvgAnnualIncome - 50_000m) * 1.5m; // Smaller holdings
            var monthlyGains = assetHoldings * Math.Max(0m, assetPriceIndex - 1m) / 12m;
            
            return monthlyGains * taxRate;
        }

        static decimal CalculatePropertyTax(PopulationCohort cohort, decimal taxRate)
        {
            // Property values roughly correlated with income
            var estimatedPropertyValue = Math.Min(1_000_000m, cohort.AvgAnnualIncome * 2.5m); // Smaller multiplier
            if (cohort.AvgAnnualIncome < 25_000m) estimatedPropertyValue *= 0.3m; 
            
            return estimatedPropertyValue * taxRate / 12m;
        }

        // ==========================================
        // LABOR MARKET DYNAMICS - FIXED
        // ==========================================

        static (decimal unemploymentRate, decimal wageGrowth, decimal laborSupply) UpdateLaborMarket(
            List<PopulationCohort> cohorts,
            Dictionary<string, int> populations,
            UbiProgram ubi,
            TaxPolicy tax,
            decimal currentUnemploymentRate,
            LaborMarket labor,
            decimal priceLevel,
            int month,
            bool advancedMode)
        {
            const decimal trendProductivity = 0.0015m; // ~0.15%/month ≈ 1.8%/yr
            // FIXED: Calculate UBI effect on unemployment properly
            var ubiLaborSupplyEffect = CalculateUbiLaborSupplyEffect(cohorts, populations, ubi, advancedMode);
            var ubiEffect = 0.30m * ubiLaborSupplyEffect;    
            if (ubi.MonthlyUbiPerAdult > 0m)
            {
                // UBI-to-median-income ratio affects labor supply
                var medianIncome = cohorts.OrderBy(c => c.AvgAnnualIncome).ElementAt(3).AvgAnnualIncome; // Middle cohort
                var ubiToIncomeRatio = (ubi.MonthlyUbiPerAdult * 12m) / medianIncome;
                ubiEffect = ubiToIncomeRatio * 0.03m; // 3% unemployment increase per 100% UBI-to-income ratio
                ubiEffect = Clamp(ubiEffect, 0m, 0.04m); // Max 4% unemployment increase
            }
            
            var uStar = Clamp(
                labor.UnemploymentRate                           // your baseline (e.g., 4.5%)
                + 0.5m * labor.SkillMismatchFactor                 // a little higher if mismatch
                - 0.05m * labor.UnionCoverage,                     // unions can compress u*
                0.035m, 0.065m);
               
            var effectiveTaxRate = CalculateAvgEffectiveTaxRate(cohorts, populations, tax);

            // Tax effect on unemployment
            var avgTaxRate = CalculateAvgEffectiveTaxRate(cohorts, populations, tax);
            var taxGap = avgTaxRate - 0.25m;
            var taxEffect = 0.05m * taxGap;
            taxEffect = Clamp(taxEffect, -0.002m, 0.010m);

            // Target unemployment rate
            var baseUnemploymentRate = labor.UnemploymentRate; // 4.5%
            var targetUnemploymentRate = Clamp(uStar + ubiEffect + taxEffect, 0.035m, 0.090m);
            //var targetUnemploymentRate = baseUnemploymentRate + ubiEffect + taxEffect;
            targetUnemploymentRate = Clamp(targetUnemploymentRate, 0.025m, 0.12m); // 2.5% to 12%

            // FIXED: Gradual adjustment (was not working before)
            //var adjustmentSpeed = 0.15m; // 15% adjustment per month
            //var newUnemploymentRate = currentUnemploymentRate * (1m - adjustmentSpeed) + targetUnemploymentRate * adjustmentSpeed;
            var adjustmentSpeed = 0.04m; // not 0.15
            var newUnemploymentRate =
                currentUnemploymentRate + adjustmentSpeed * (targetUnemploymentRate - currentUnemploymentRate);
            // Ensure unemployment actually changes
            if (Math.Abs(newUnemploymentRate - currentUnemploymentRate) < 0.001m)
            {
                newUnemploymentRate = currentUnemploymentRate + 
                    Math.Sign(targetUnemploymentRate - currentUnemploymentRate) * 0.001m;
            }

            // Wage growth calculation
            var unemploymentGap = newUnemploymentRate - uStar;
            //var unemploymentGap = newUnemploymentRate - baseUnemploymentRate;
            // Final wage growth = trend productivity + tightness effect, lightly bounded
            var wagePhillips = -0.05m * unemploymentGap;
            var wageGrowth = Clamp(trendProductivity + wagePhillips, -0.003m, 0.006m); // [-0.3%, +0.6%]/mo
            //var wageGrowth = -unemploymentGap * 0.5m; // Phillips curve relationship
            wageGrowth = Clamp(wageGrowth, -0.03m, 0.04m);

            var totalLaborForce = populations.Values.Sum() * 0.65m;
            var adjustedLaborSupply = totalLaborForce * (1m - ubiEffect * 0.5m);

            ////////Console.WriteLine($"[DEBUG] Month {month}: UBI=${ubi.MonthlyUbiPerAdult}, " + $"UbiEffect={ubiEffect:P2}, TaxEffect={taxEffect:P2}, " + $"Target={targetUnemploymentRate:P2}, New={newUnemploymentRate:P2}");

            return (newUnemploymentRate, wageGrowth, adjustedLaborSupply);
        }
        
        static decimal CalculateUbiLaborSupplyEffect(
            List<PopulationCohort> cohorts, Dictionary<string, int> populations, UbiProgram ubi, bool advancedMode)
        {
            if (!advancedMode || ubi.MonthlyUbiPerAdult == 0m) return 0m;
            
            decimal weightedEffect = 0m;
            decimal totalPopulation = 0m;
            
            foreach (var cohort in cohorts)
            {
                var population = populations[cohort.Name];
                var ubiToIncomeRatio = (ubi.MonthlyUbiPerAdult * 12m) / Math.Max(10000m, cohort.AvgAnnualIncome);
                
                // Stronger effect for lower-income groups, but more realistic
                var cohortEffect = Math.Min(0.08m, ubiToIncomeRatio * 0.15m); // Smaller max effect
                
                // Adjust for demographics (smaller effects)
                if (cohort.AgeMedian > 55m) cohortEffect *= 1.3m; 
                if (cohort.AgeMedian < 30m) cohortEffect *= 0.8m; 
                
                weightedEffect += cohortEffect * population;
                totalPopulation += population;
            }
            
            return totalPopulation > 0 ? weightedEffect / totalPopulation : 0m;
        }

        // ==========================================
        // BUSINESS SECTOR DYNAMICS - FIXED TO INCLUDE CORPORATE TAX
        // ==========================================

        static (BusinessEcosystem newBusiness, decimal totalInvestment, decimal totalProfit, decimal corporateTax) UpdateBusinessSector(
            BusinessEcosystem currentBusiness,
            decimal unemploymentRate,
            decimal interestRate,
            decimal priceLevel,
            FinancialSector financial,
            TaxPolicy tax,
            UbiProgram ubi,
            bool stochastic,
            bool advancedMode)
        {
            var newFirmSizes = new Dictionary<string, FirmSize>();
            decimal totalInvestment = 0m;
            decimal totalProfit = 0m;
            decimal corporateTax = 0m;

            //Console.WriteLine($"[DEBUG] Business Sector: Corp Rate={tax.CorpTaxRate:P1}, Unemployment={unemploymentRate:P1}");

            foreach (var (category, firmSize) in currentBusiness.FirmSizes)
            {
                // FIXED: Much more realistic profit calculations
                var baseMargin = CalculateBaseProfitMargin(category, unemploymentRate, priceLevel);
                baseMargin = Clamp(baseMargin, 0.03m, 0.25m);

                // FIXED: Enhanced scale factors to match US economy
                var scaleFactor = category switch
                {
                    "Small" => 6.0m,    // Small businesses scaled up significantly
                    "Medium" => 12.0m,  // Medium businesses scaled up significantly  
                    "Large" => 20.0m,   // Large businesses scaled up significantly
                    _ => 1.0m
                };

                // FIXED: Calculate realistic revenue streams
                var baseAnnualRevenue = (decimal)firmSize.Count * firmSize.AvgRevenue;
                var scaledAnnualRevenue = baseAnnualRevenue * scaleFactor;
                var monthlyRevenue = scaledAnnualRevenue / 12m;
                
                // Add UBI demand boost (people spend more)
                if (ubi.MonthlyUbiPerAdult > 0m)
                {
                    var ubiBoost = 1m + (ubi.MonthlyUbiPerAdult / 1000m) * 0.02m; // 2% boost per $1000 UBI
                    monthlyRevenue *= ubiBoost;
                }
                
                // Cap revenue to prevent overflow but allow larger values
                monthlyRevenue = Math.Min(5_000_000_000_000m, monthlyRevenue); // $5T monthly cap

                var profit = monthlyRevenue * baseMargin;
                profit = Clamp(profit, 0m, 1_000_000_000_000m); // $1T monthly profit cap

                // FIXED: Corporate tax calculation with proper rates
                var firmCorporateTax = profit * tax.CorpTaxRate;
                corporateTax += firmCorporateTax;

                // After-tax profit for investment
                var afterTaxProfit = profit - firmCorporateTax;
                var investmentRate = CalculateInvestmentRate(firmSize, afterTaxProfit, interestRate, financial);
                var investment = afterTaxProfit * investmentRate;

                // Firm dynamics (keep stable for now)
                var newCount = firmSize.Count;
                var productivityGrowth = firmSize.ProductivityGrowth;
                
                if (ubi != null && ubi.MonthlyUbiPerAdult > 600m)
                    productivityGrowth += 0.0008m; // UBI productivity boost

                newFirmSizes[category] = firmSize with 
                { 
                    Count = newCount,
                    ProductivityGrowth = Clamp(productivityGrowth, -0.005m, 0.025m)
                };

                totalInvestment += investment;
                totalProfit += profit;

                //Console.WriteLine($"[DEBUG] {category}: Firms={firmSize.Count:N0}, " +$"Revenue=${monthlyRevenue/1_000_000_000m:F1}B, " + $"Margin={baseMargin:P1}, Profit=${profit/1_000_000_000m:F1}B, " + $"CorpTax=${firmCorporateTax/1_000_000_000m:F1}B");
            }

            corporateTax = Clamp(corporateTax, 0m, 1_500_000_000_000m); // $1.5T monthly cap

            //Console.WriteLine($"[DEBUG] TOTAL: Profit=${totalProfit/1_000_000_000m:F1}B, " +$"Corp Tax=${corporateTax/1_000_000_000m:F1}B " + $"(Rate: {tax.CorpTaxRate:P1})");

            var newBusiness = currentBusiness with { FirmSizes = newFirmSizes };
            return (newBusiness, totalInvestment, totalProfit, corporateTax);
        }
        
        static decimal CalculateBaseProfitMargin(string firmCategory, decimal unemploymentRate, decimal priceLevel)
        {
            // Base margins vary by firm size
            decimal baseMargin = firmCategory switch
            {
                "Small" => 0.06m,      
                "Medium" => 0.08m,      
                "Large" => 0.10m,      
                _ => 0.08m
            };
            
            // Economic conditions affect margins (smaller effects)
            var unemploymentBonus = unemploymentRate * 0.2m; // Smaller wage effect
            var priceEffect = (priceLevel - 1m) * 0.1m; // Smaller price effect
            
            return Clamp(baseMargin + unemploymentBonus + priceEffect, 0.02m, 0.15m);
        }

        static decimal CalculateInvestmentRate(FirmSize firmSize, decimal profit, 
            decimal interestRate, FinancialSector financial)
        {
            // Investment rate depends on profitability vs cost of capital
            var baseInvestmentRate = 0.2m; // 20% of profits reinvested
            
            // Interest rate sensitivity (smaller effect)
            var interestSensitivity = firmSize.Category == "Small" ? -1.0m : -0.5m;
            var interestEffect = interestSensitivity * (interestRate - financial.BaseInterestRate);
            
            // Credit availability (smaller effect)
            var creditEffect = (financial.BankLendingCapacity - 1m) * 0.2m;
            
            return Clamp(baseInvestmentRate + interestEffect + creditEffect, 0.05m, 0.40m);
        }

        static (decimal entryRate, decimal exitRate) CalculateEntryExitRates(
            FirmSize firmSize, decimal profitMargin, bool advancedMode)
        {
            if (!advancedMode)
            {
                // Simple model (smaller rates)
                var simpleExitRate = profitMargin < 0.05m ? 0.005m : 0.002m;
                var simpleEntryRate = profitMargin > 0.10m ? 0.004m : 0.001m;
                return (simpleEntryRate, simpleExitRate);
            }
            
            // Enhanced model with entry barriers and exit sensitivity (smaller effects)
            var baseExitRate = 0.001m;
            var profitExitEffect = profitMargin < 0.05m ? 
                (0.05m - profitMargin) * firmSize.ExitSensitivity * 0.5m : 0m; // Smaller effect
            var calculatedExitRate = Clamp(baseExitRate + profitExitEffect, 0m, 0.02m);
            
            var baseEntryRate = 0.002m / Math.Max(1m, firmSize.EntryBarrier); 
            var profitEntryEffect = Math.Max(0m, profitMargin - 0.08m) * 1m; // Smaller effect
            var calculatedEntryRate = Clamp(baseEntryRate + profitEntryEffect, 0m, 0.01m);
            
            return (calculatedEntryRate, calculatedExitRate);
        }

        // ==========================================
        // FINANCIAL MARKETS - FIXED TAYLOR RULE
        // ==========================================

        static (decimal newInterestRate, decimal newAssetPrices, decimal creditGrowth) UpdateFinancialMarkets(
            decimal currentRate, decimal inflation, decimal unemployment, decimal outputGap,
            decimal currentAssetPrices, FinancialSector financial, EconomyParams econ, 
            decimal cumulativeInflation, bool advancedMode)
        {
            if (!advancedMode)
            {
                var targetRate = financial.BaseInterestRate + inflation * 1.5m;
                var newRate = currentRate * 0.8m + targetRate * 0.2m;
                return (newRate, currentAssetPrices * 1.002m, 0.02m);
            }

            // FIXED: Correct Taylor rule implementation
            var inflationTarget = 0.02m; // 2% annual target
            var naturalRate = 0.025m; // 2.5% annual natural rate
            
            // FIXED: Calculate proper annualized inflation from cumulative price level
            var annualizedInflation = (cumulativeInflation - 1m) * 4m; // Annualize from 3-year cumulative
            if (cumulativeInflation <= 1m) annualizedInflation = inflation * 12m; // Fallback to monthly * 12
            
            // FIXED: Proper unemployment gap
            var unemploymentTarget = 0.045m; // 4.5% natural rate
            var unemploymentGap = unemployment - unemploymentTarget;
            
            // FIXED: More aggressive Taylor rule for realistic rates
            var taylorRate = naturalRate + annualizedInflation + 
                            2.0m * (annualizedInflation - inflationTarget) +       // Strong inflation response
                            1.0m * (-unemploymentGap) +                          // Strong unemployment response  
                            0.5m * outputGap;                                    // Output gap response
            
            // FIXED: Ensure rates are reasonable but responsive
            taylorRate = Clamp(taylorRate, 0.01m, 0.12m); // 1% to 12% annually (realistic bounds)
            
            // FIXED: More responsive adjustment to reach target faster
            var rateAdjustmentSpeed = 0.3m; // 30% adjustment per month
            var monthlyTaylorRate = taylorRate / 12m; // Convert annual to monthly
            var newInterestRate = currentRate * (1m - rateAdjustmentSpeed) + 
                                 monthlyTaylorRate * rateAdjustmentSpeed;
            
            // FIXED: Proper monthly bounds
            newInterestRate = Clamp(newInterestRate, 0.01m / 12m, 0.12m / 12m); // Monthly equivalent of 1-12% annual

            //Console.WriteLine($"[DEBUG] Taylor Rule: CumInflation={cumulativeInflation:F3}, " +$"AnnualInflation={annualizedInflation:P1}, UnempGap={unemploymentGap:P1}, " + $"TaylorTarget={taylorRate:P1}, CurrentRate={currentRate*12m:P1}, " + $"NewRate={newInterestRate*12m:P1}");

            // Asset prices and credit growth
            var assetPriceGrowth = -0.6m * (newInterestRate - currentRate) * 12m + 0.3m * outputGap;
            if (stochastic) assetPriceGrowth += 0.01m * ((decimal)Rng.NextDouble() - 0.5m);
            var newAssetPrices = currentAssetPrices * (1m + Clamp(assetPriceGrowth, -0.03m, 0.03m));

            var baseCreditGrowth = 0.02m;
            var rateCreditEffect = -2.5m * (newInterestRate - financial.BaseInterestRate);
            var creditGrowth = Clamp(baseCreditGrowth + rateCreditEffect, -0.06m, 0.10m);

            return (newInterestRate, newAssetPrices, creditGrowth);
        }

        // ==========================================
        // INTERNATIONAL TRADE
        // ==========================================

        static decimal CalculateNetExports(TradeBalance trade, decimal priceLevel, decimal exchangeRate)
        {
            // Simple net exports calculation
            var competitiveness = 1m / Math.Max(0.5m, priceLevel * exchangeRate);
            var competitivenessChange = competitiveness - 1m;
            
            var adjustedExports = trade.ExportLevel * (1m + trade.ExportElasticity * competitivenessChange);
            var adjustedImports = trade.ImportLevel * (1m - trade.ImportElasticity * competitivenessChange);
            
            return adjustedExports - adjustedImports;
        }

        static (decimal exports, decimal imports, decimal newExchangeRate) UpdateInternationalTrade(
            TradeBalance trade, decimal priceLevel, decimal nominalGDP, decimal currentExchangeRate, bool advancedMode)
        {
            if (!advancedMode)
            {
                // Simple fixed trade flows
                return (trade.ExportLevel, trade.ImportLevel, currentExchangeRate);
            }
            
            // Competitiveness effects
            var competitiveness = 1m / Math.Max(0.5m, priceLevel * currentExchangeRate); 
            var competitivenessChange = competitiveness - 1m;
            
            // Export response to competitiveness
            var newExports = trade.ExportLevel * (1m + trade.ExportElasticity * competitivenessChange);
            newExports *= (1m + trade.ForeignDemandGrowth / 12m); 
            
            // Import response to domestic demand and competitiveness  
            var domesticDemandEffect = (nominalGDP / 1_500_000_000_000m) - 1m; 
            var newImports = trade.ImportLevel * 
                           (1m + 0.5m * domesticDemandEffect) * 
                           (1m - trade.ImportElasticity * competitivenessChange); 
            
            // Exchange rate adjustment (smaller effect)
            var tradeBalanceChange = newExports - newImports;
            var exchangeRateAdjustment = tradeBalanceChange * 0.000000000005m; // Smaller effect
            var newExchangeRate = currentExchangeRate * (1m + Clamp(exchangeRateAdjustment, -0.005m, 0.005m));
            
            return (newExports, newImports, newExchangeRate);
        }

        // ==========================================
        // MIGRATION AND EMIGRATION - FIXED TO SHOW EFFECTS
        // ==========================================

        static (int emigrants, int immigrants) UpdateMigrationFlows(
            List<PopulationCohort> cohorts,
            Dictionary<string, int> populations,
            TaxPolicy tax,
            UbiProgram ubi,
            decimal unemploymentRate,
            EmigrationParams emig,
            bool stochastic,
            bool advancedMode)
        {
            int totalEmigrants = 0;
            int totalImmigrants = 0;

            foreach (var cohort in cohorts)
            {
                var population = populations[cohort.Name];
                if (population <= 0) continue;

                // FIXED: More balanced emigration calculation
                var effectiveTaxRate = CalculateEffectiveTaxRate(cohort.AvgAnnualIncome, tax.Brackets);
                
                // Tax pressure increases emigration (especially for wealthy)
                var taxPressure = 0m;
                if (cohort.AvgAnnualIncome > 500_000m) // Very wealthy
                {
                    taxPressure = Math.Max(0m, effectiveTaxRate - 0.15m) * 3.0m; // Very sensitive above 15%
                }
                else if (cohort.AvgAnnualIncome > 200_000m) // Wealthy cohorts
                {
                    taxPressure = Math.Max(0m, effectiveTaxRate - 0.25m) * 2.0m; // Sensitive above 25%
                }
                else if (cohort.AvgAnnualIncome > 100_000m) // Upper middle class
                {
                    taxPressure = Math.Max(0m, effectiveTaxRate - 0.35m) * 1.0m; // Sensitive above 35%
                }
                else // Lower/middle income
                {
                    taxPressure = Math.Max(0m, effectiveTaxRate - 0.45m) * 0.5m; // Only extreme taxes
                }

                // FIXED: UBI reduces emigration but doesn't eliminate it
                var ubiAttraction = 0m;
                if (ubi.MonthlyUbiPerAdult > 0m)
                {
                    var ubiToIncomeRatio = (ubi.MonthlyUbiPerAdult * 12m) / Math.Max(25000m, cohort.AvgAnnualIncome);
                    // FIXED: Cap UBI attraction at 60% reduction (not 80%)
                    ubiAttraction = Math.Min(0.6m, ubiToIncomeRatio * emig.UbiAttraction * 0.8m);
                }

                // Base emigration varies by mobility/income  
                var baseEmigrationProb = emig.BaseMonthlyProbByIncome * cohort.GeographicMobility;
                
                // Unemployment increases emigration desire
                var unemploymentPressure = Math.Max(0m, unemploymentRate - 0.05m) * 0.3m; // Above 5% unemployment

                // FIXED: Ensure minimum baseline emigration even with UBI
                var minEmigrationProb = baseEmigrationProb * 0.2m; // At least 20% of baseline always remains
                
                var emigrationProb = baseEmigrationProb + taxPressure + (effectiveTaxRate > 0.3m ? (effectiveTaxRate - 0.3m) * emig.TaxSensitivity : 0m) - ubiAttraction + unemploymentPressure; // Threshold 0.3 per IMF
                
                // Net emigration probability
                //var emigrationProb = baseEmigrationProb + taxPressure * emig.TaxSensitivity - ubiAttraction + unemploymentPressure;
                
                // FIXED: Ensure emigration never goes below minimum baseline
                emigrationProb = Math.Max(minEmigrationProb, emigrationProb);
                emigrationProb = Clamp(emigrationProb, minEmigrationProb, 0.004m); // Max 0.4% monthly

                // Immigration - UBI makes country more attractive
                var immigrationMultiplier = 1m + ubiAttraction * 0.4m; // Moderate immigration boost
                var immigrationProb = baseEmigrationProb * 0.15m * immigrationMultiplier;

                // Apply calculations
                int cohortEmigrants = stochastic ? 
                    BinomialSample(population, emigrationProb) : 
                    BinomialExpected(population, emigrationProb);

                int cohortImmigrants = stochastic ?
                    BinomialSample((int)(population * 0.01m), immigrationProb) : 
                    BinomialExpected((int)(population * 0.01m), immigrationProb);

                // Update population
                populations[cohort.Name] = Math.Max(100, population - cohortEmigrants + cohortImmigrants);

                totalEmigrants += cohortEmigrants;
                totalImmigrants += cohortImmigrants;

                if (cohortEmigrants > 0 || cohortImmigrants > 0)
                {
                    //Console.WriteLine($"[DEBUG] {cohort.Name}: Tax={effectiveTaxRate:P1}, " +$"TaxPressure={taxPressure:P2}, UbiAttraction={ubiAttraction:P2}, " + $"MinEmig={minEmigrationProb:P3}, FinalEmig={emigrationProb:P3}, " + $"Emigrants={cohortEmigrants}, Immigrants={cohortImmigrants}");
                }
            }

            //Console.WriteLine($"[DEBUG] Total Migration: Emigrants={totalEmigrants}, Immigrants={totalImmigrants}");

            return (totalEmigrants, totalImmigrants);
        }

        // ==========================================
        // REGIONAL ECONOMICS
        // ==========================================

        static RegionalEconomy UpdateRegionalEconomics(
            RegionalEconomy regional, decimal nationalUnemployment, 
            Dictionary<string, int> populations, decimal priceLevel)
        {
            var newRegions = new Dictionary<string, RegionalData>();
            
            foreach (var (regionName, regionData) in regional.Regions)
            {
                // Regional unemployment varies around national average
                var regionalUnemployment = nationalUnemployment * 
                    (0.8m + 0.4m * (2m - regionData.ProductivityLevel)); 
                regionalUnemployment = Clamp(regionalUnemployment, nationalUnemployment * 0.7m, nationalUnemployment * 1.3m);
                
                newRegions[regionName] = regionData with 
                { 
                    UnemploymentRate = regionalUnemployment 
                };
            }
            
            return regional with { Regions = newRegions };
        }

        // ==========================================
        // UTILITY FUNCTIONS - FIXED INFLATION CALCULATION
        // ==========================================

        static decimal CalculateNetFiscalPosition(List<EnhancedMonthResult> results)
        {
            return results.Sum(r => r.TotalTaxes - r.UbiOutlays - r.OtherGovernmentSpending);
        }

        static decimal CalculateInflation(decimal outputGap, decimal priceLevel, EconomyParams econ, 
            decimal cumulativeInflation, int month)
        {
            // FIXED: Much more stable inflation calculation
            var phillipsCurveInflation = outputGap * 0.1m / 100m; // 0.1pp inflation per 1pp output gap (was 0.2pp)
    
            // FIXED: Less persistent inflation
            var recentInflation = month > 1 ? (cumulativeInflation - 1m) / month : 0m; 
            var persistentInflation = recentInflation * econ.InflationPersistence * 0.1m; // Much smaller persistence
    
            // Trend inflation
            var trendInflation = 0.02m / 12m; // 2% annual target = ~0.167% monthly
    
            // FIXED: Much more stable total inflation
            var totalInflation = trendInflation + phillipsCurveInflation + persistentInflation;
    
            // FIXED: Tighter bounds to prevent instability
            var boundedInflation = Clamp(totalInflation, -0.005m, 0.005m); // Max ±0.5% monthly = ±6% annually
    
            //////Console.WriteLine($"[DEBUG] Inflation: OutputGap={outputGap:P2}, Phillips={phillipsCurveInflation:P3}, " +$"Persistent={persistentInflation:P3}, Total={boundedInflation:P3}");
    
            return boundedInflation;
        }

        static decimal CalculateTotalCapacity(BusinessEcosystem business)
        {
            // Simplified capacity measure based on firm productivity
            return 1.0m; // For now, assume constant capacity
        }

        static decimal CalculateEffectiveTaxRate(decimal annualIncome, List<TaxBracket> brackets)
        {
            var tax = CalculatePersonalIncomeTax(annualIncome, brackets);
            return annualIncome > 0 ? tax / annualIncome : 0m;
        }

        static decimal CalculatePersonalIncomeTax(decimal annualIncome, List<TaxBracket> brackets)
        {
            decimal tax = 0m;
            var sortedBrackets = brackets.OrderBy(b => b.Threshold).ToList();
            
            for (int i = 0; i < sortedBrackets.Count; i++)
            {
                var bracket = sortedBrackets[i];
                var nextThreshold = i < sortedBrackets.Count - 1 ? 
                    sortedBrackets[i + 1].Threshold : decimal.MaxValue;
                
                if (annualIncome > bracket.Threshold)
                {
                    var taxableInThisBracket = Math.Min(annualIncome, nextThreshold) - bracket.Threshold;
                    tax += taxableInThisBracket * bracket.Rate;
                }
            }
            
            return Math.Max(0m, tax);
        }

        static decimal CalculateAvgEffectiveTaxRate(List<PopulationCohort> cohorts, 
            Dictionary<string, int> populations, TaxPolicy tax)
        {
            decimal weightedTaxRate = 0m;
            decimal totalPopulation = 0m;
            
            foreach (var cohort in cohorts)
            {
                var population = populations[cohort.Name];
                var etr = CalculateEffectiveTaxRate(cohort.AvgAnnualIncome, tax.Brackets);
                weightedTaxRate += etr * population;
                totalPopulation += population;
            }
            
            return totalPopulation > 0 ? weightedTaxRate / totalPopulation : 0m;
        }

        static decimal CalculateAvgWageLevel(List<PopulationCohort> cohorts, decimal wageGrowth)
        {
            return cohorts.Average(c => c.AvgAnnualIncome) * (1m + wageGrowth) / 12m;
        }

        static decimal CalculateLaborForceParticipation(List<PopulationCohort> cohorts, 
            Dictionary<string, int> populations)
        {
            decimal weightedParticipation = 0m;
            decimal totalPopulation = 0m;
            
            foreach (var cohort in cohorts)
            {
                var population = populations[cohort.Name];
                weightedParticipation += cohort.WorkParticipationRate * population;
                totalPopulation += population;
            }
            
            return totalPopulation > 0 ? weightedParticipation / totalPopulation : 0.65m;
        }

        static decimal CalculateTotalSavings(List<PopulationCohort> cohorts, 
            Dictionary<string, int> populations, decimal totalConsumption)
        {
            decimal totalIncome = 0m;
            
            foreach (var cohort in cohorts)
            {
                var population = populations[cohort.Name];
                totalIncome += (cohort.AvgAnnualIncome / 12m) * population;
            }
            
            return Math.Max(0m, totalIncome - totalConsumption);
        }

        static decimal Clamp(decimal value, decimal min, decimal max)
        {
            return Math.Min(max, Math.Max(min, value));
        }

        static int BinomialExpected(int n, decimal p)
        {
            return (int)Math.Round(n * Clamp(p, 0m, 1m));
        }

        static int BinomialSample(int n, decimal p)
        {
            p = Math.Max(0m, Math.Min(0.999999m, p)); 
            if (n <= 1000) // Reduced threshold for speed
            {
                // Exact binomial sampling for small n
                int x = 0;
                double pd = (double)p;
                for (int i = 0; i < n; i++) 
                    if (Rng.NextDouble() < pd) x++;
                return x;
            }
            
            // Normal approximation for large n
            double pp = (double)p;
            double mean = n * pp;
            double variance = mean * (1.0 - pp);
            double std = Math.Sqrt(Math.Max(variance, 1e-9));
            
            int draw = (int)Math.Round(mean + SampleStandardNormal() * std);
            return Math.Max(0, Math.Min(n, draw)); 
        }

        static double SampleStandardNormal()
        {
            // Box-Muller transform for standard normal distribution
            double u1 = 1.0 - Rng.NextDouble(); 
            double u2 = 1.0 - Rng.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        }

        // ==========================================
        // DATA SETUP AND CLONING - SMALLER VALUES
        // ==========================================

        static (List<PopulationCohort> cohorts, LaborMarket labor, BusinessEcosystem business,
                FinancialSector financial, TradeBalance trade, RegionalEconomy regional,
                GovernmentSector government, EconomyParams econ, EmigrationParams emig) BuildEnhancedEconomy()
        {
            var totalAdults = 260_000_000; 

            var cohorts = new List<PopulationCohort>
            {
                new("Bottom 20%", 15_000m, 0.00m, (int)(totalAdults * 0.20), 0.55m, 0.95m, 0.10m, 0.05m, 38m, 0.3m, "South"),
                new("Lower-Mid 20%", 35_000m, 0.02m, (int)(totalAdults * 0.20), 0.70m, 0.90m, 0.15m, 0.15m, 42m, 0.5m, "Midwest"),
                new("Middle 20%", 65_000m, 0.05m, (int)(totalAdults * 0.20), 0.80m, 0.85m, 0.20m, 0.25m, 45m, 0.6m, "Midwest"),
                new("Upper-Mid 20%", 100_000m, 0.12m, (int)(totalAdults * 0.20), 0.85m, 0.75m, 0.30m, 0.40m, 47m, 0.7m, "West"),
                new("Top 15%", 180_000m, 0.25m, (int)(totalAdults * 0.15), 0.90m, 0.70m, 0.50m, 0.65m, 49m, 0.8m, "Northeast"),
                new("Top 4%", 350_000m, 0.40m, (int)(totalAdults * 0.04), 0.85m, 0.60m, 0.70m, 0.80m, 52m, 0.9m, "Northeast"),
                new("Top 1%", 1_200_000m, 0.55m, (int)(totalAdults * 0.01), 0.80m, 0.50m, 0.90m, 0.95m, 55m, 0.95m, "Northeast")
            };

            var labor = new LaborMarket(
                UnemploymentRate: 0.045m,
                JobSearchFriction: 3.5m, 
                SkillMismatchFactor: 0.02m,
                WageStickinessDown: 0.7m, // Reduced stickiness for more responsiveness
                WageStickinessUp: 0.5m,
                UnionCoverage: 0.11m,
                RegionalWageGap: new Dictionary<string, decimal>
                {
                    ["Northeast"] = 1.15m,
                    ["West"] = 1.10m, 
                    ["Midwest"] = 0.95m,
                    ["South"] = 0.90m
                }
            );

            var business = new BusinessEcosystem(
                FirmSizes: new Dictionary<string, FirmSize>
                {
                    // FIXED: Higher base revenues for realistic corporate tax
                    ["Small"] = new("Small", 6_000_000, 8m, 250_000m, 2.0m, 2.0m, 0.005m, 0.8m, 0.01m),    // $250K avg revenue
                    ["Medium"] = new("Medium", 300_000, 95m, 8_000_000m, 5.0m, 1.5m, 0.008m, 0.6m, 0.012m), // $8M avg revenue  
                    ["Large"] = new("Large", 8_000, 1200m, 200_000_000m, 15.0m, 1.0m, 0.012m, 0.4m, 0.015m)  // $200M avg revenue
                },
                NetworkEffects: 0.15m,
                InnovationSpillover: 0.02m,
                GlobalCompetition: 0.30m,
                SupplyChainIntegration: 0.75m
            );

            var financial = new FinancialSector(
                BaseInterestRate: 0.025m,
                CreditSpread: 0.02m,
                CreditElasticity: 0.6m,
                SavingsElasticity: 0.3m,
                AssetPriceLevel: 1.0m,
                BankLendingCapacity: 1.0m,
                FinancialStabilityIndex: 1.0m
            );

            var trade = new TradeBalance(
                ExportLevel: 80_000_000_000m, // Reduced 
                ImportLevel: 90_000_000_000m, // Reduced 
                ExportElasticity: 1.0m,
                ImportElasticity: 0.6m,
                ExchangeRate: 1.0m,
                ForeignDemandGrowth: 0.02m, 
                TradeAgreementEffect: 1.0m
            );

            var regional = new RegionalEconomy(
                Regions: new Dictionary<string, RegionalData>
                {
                    ["Northeast"] = new("Northeast", 0.18m, 1.15m, 1.20m, 0.040m, 1.40m, 0.08m, 1.1m),
                    ["Midwest"] = new("Midwest", 0.21m, 1.00m, 0.85m, 0.045m, 0.85m, 0.06m, 0.9m),
                    ["South"] = new("South", 0.38m, 0.95m, 0.80m, 0.042m, 0.75m, 0.05m, 1.0m),
                    ["West"] = new("West", 0.23m, 1.20m, 1.35m, 0.048m, 1.60m, 0.07m, 1.2m)
                },
                LaborMobility: 0.15m,
                CapitalMobility: 0.25m,
                RegionalMultiplier: 1.3m,
                TransportationCosts: 0.02m
            );

            var government = new GovernmentSector(
                BaselineSpending: 6_750_000_000_000m, // Realistic level 6.75 trilion based off FY 2024 figures 
                UnemploymentBenefitRate: 0.45m,
                SocialSecurityLevel: 80_000_000_000m, // Reduced 
                PublicInvestmentRate: 0.03m,
                DebtToGDPRatio: 1.20m,
                AutomaticStabilizerStrength: 0.4m
            );

            var econ = new EconomyParams(
                BaselineMonthlyGDP: 1_500_000_000_000m, 
                PriceAdjustment: 0.15m, // Reduced for stability
                WageResponse: 0.03m,
                ProductivityGrowth: 0.0015m, 
                InflationPersistence: 0.5m, // Reduced for stability
                ExpectationAdaptation: 0.15m,
                MonetaryPolicyRule: 1.5m 
            );

            var emig = new EmigrationParams(
                BaseMonthlyProbByIncome: 0.0001m,    // Lower baseline (was 0.0002m)
                TaxSensitivity: 0.006m,              // Higher tax sensitivity (was 0.004m)
                UbiAttraction: 0.8m,                 // Higher UBI attraction (was 0.4m)
                QualityOfLifeEffect: 0.3m,
                NetworkEffects: 0.15m,
                ReentryProbability: 0.1m
            );

            return (cohorts, labor, business, financial, trade, regional, government, econ, emig);
        }

        static List<PopulationCohort> CloneCohorts(List<PopulationCohort> source)
        {
            return source.Select(c => c with { }).ToList(); 
        }

        static BusinessEcosystem CloneBusinessEcosystem(BusinessEcosystem source)
        {
            return source with 
            { 
                FirmSizes = source.FirmSizes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value with { })
            };
        }

        static RegionalEconomy CloneRegionalEconomy(RegionalEconomy source)
        {
            return source with
            {
                Regions = source.Regions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value with { })
            };
        }

        // ==========================================
        // OUTPUT AND REPORTING
        // ==========================================

        static void PrintEnhancedSummary(List<EnhancedMonthResult> results)
        {
            Console.WriteLine($"\n=== Enhanced UBI Simulation Results ({results.Count} months) ===");
            Console.WriteLine("Month | Price | RealGDP | Unemp% | IntRate | PIT($B) | VAT($B) | Corp($B) | UBI($B) | Net($B) | Firms(M) | Trade($B)");
            
            foreach (var r in results.Take(6).Concat(results.TakeLast(6)))
            {
                var net = r.TotalTaxes - r.UbiOutlays - r.OtherGovernmentSpending;
                Console.WriteLine($"{r.MonthIndex,5} | {r.PriceLevel,5:F2} | {r.RealGDP/1_000_000_000_000m,6:F1}T | " +
                                  $"{r.UnemploymentRate,5:P1} | {r.InterestRate,6:P1} | {r.PersonalIncomeTax/1_000_000_000m,6:F0} | " +
                                  $"{r.VatRevenue/1_000_000_000m,6:F0} | {r.CorpTax/1_000_000_000m,7:F0} | " +
                                  $"{r.UbiOutlays/1_000_000_000m,6:F0} | {net/1_000_000_000m,6:F0} | " +
                                  $"{r.FirmsBySize.Values.Sum()/1_000_000m,7:F1} | {r.TradeBalance/1_000_000_000m,8:F0}");
            }
            
            var totalTax = results.Sum(r => r.TotalTaxes);
            var totalUbi = results.Sum(r => r.UbiOutlays);
            var totalSpending = results.Sum(r => r.OtherGovernmentSpending);
            var netPosition = totalTax - totalUbi - totalSpending;
            
            Console.WriteLine("\n--- Summary Statistics ---");
            Console.WriteLine($"Total tax revenue:     ${totalTax:N0}");
            Console.WriteLine($"Total UBI outlays:     ${totalUbi:N0}");
            Console.WriteLine($"Other gov spending:    ${totalSpending:N0}");
            Console.WriteLine($"Net fiscal position:   ${netPosition:N0}");
            Console.WriteLine($"Final unemployment:    {results.Last().UnemploymentRate:P1}");
            Console.WriteLine($"Final price level:     {results.Last().PriceLevel:F3}");
            Console.WriteLine($"Final interest rate:   {results.Last().InterestRate:P1}");
            Console.WriteLine($"Total emigrants:       {results.Sum(r => r.EmigrantsThisMonth):N0}");
            Console.WriteLine($"Total immigrants:      {results.Sum(r => r.ImmigrantsThisMonth):N0}");
        }

        static void WriteEnhancedMonthlyCsv(List<EnhancedMonthResult> results, string path)
        {
            using var sw = new StreamWriter(path);
            sw.WriteLine("Month,PriceLevel,NominalGDP,RealGDP,UnemploymentRate,InterestRate,ExchangeRate," +
                        "PersonalIncomeTax,VAT,CorpTax,CapitalGainsTax,PropertyTax,TotalTaxes," +
                        "UBIOutlays,OtherGovSpending,GovernmentDebt,Net," +
                        "Emigrants,Immigrants,RemainingTaxpayers,AvgETR," +
                        "SmallFirms,MediumFirms,LargeFirms,TotalInvestment,CorporateProfit,TotalCapacity," +
                        "AvgWage,LaborForceParticipation,Exports,Imports,TradeBalance," +
                        "TotalSavings,TotalConsumption,AssetPriceIndex,CreditGrowth");
            
            foreach (var r in results)
            {
                var net = r.TotalTaxes - r.UbiOutlays - r.OtherGovernmentSpending;
                sw.WriteLine(string.Join(",",
                    r.MonthIndex, r.PriceLevel.ToString("F4"), r.NominalGDP, r.RealGDP,
                    r.UnemploymentRate.ToString("F4"), r.InterestRate.ToString("F4"), r.ExchangeRate.ToString("F4"),
                    r.PersonalIncomeTax, r.VatRevenue, r.CorpTax, r.CapitalGainsTax, r.PropertyTax, r.TotalTaxes,
                    r.UbiOutlays, r.OtherGovernmentSpending, r.GovernmentDebt, net,
                    r.EmigrantsThisMonth, r.ImmigrantsThisMonth, r.RemainingTaxpayers, r.AvgEffectiveTaxRate.ToString("F6"),
                    r.FirmsBySize.GetValueOrDefault("Small", 0), r.FirmsBySize.GetValueOrDefault("Medium", 0), 
                    r.FirmsBySize.GetValueOrDefault("Large", 0), r.TotalInvestment, r.CorporateProfit, r.TotalCapacity.ToString("F4"),
                    r.AvgWageLevel, r.LaborForceParticipation.ToString("F4"), r.ExportsValue, r.ImportsValue, r.TradeBalance,
                    r.TotalSavings, r.TotalConsumption, r.AssetPriceIndex.ToString("F4"), r.CreditGrowth.ToString("F4")
                ));
            }
        }

        public record ScenarioSummary(
            string Name, int Months, decimal UBI, decimal Net, decimal PitScale, decimal CorpRate, decimal VatRate,
            decimal TotalTaxes, decimal TotalUBI, decimal TotalOtherSpending, int TotalEmigrants, int TotalImmigrants,
            int FinalTaxpayers, decimal AvgUnemployment, decimal FinalPriceLevel, decimal FinalInterestRate,
            int FinalFirms, decimal AvgTradeBalance);

        static ScenarioSummary SummarizeEnhancedScenario(
            string name, int months, UbiProgram ubi, TaxPolicy tax, 
            List<EnhancedMonthResult> results, ConvergenceInfo convergence)
        {
            return new ScenarioSummary(
                name, months, ubi.MonthlyUbiPerAdult,
                results.Sum(r => r.TotalTaxes - r.UbiOutlays - r.OtherGovernmentSpending),
                convergence.pitScale, tax.CorpTaxRate, tax.VatRate,
                results.Sum(r => r.TotalTaxes), results.Sum(r => r.UbiOutlays), results.Sum(r => r.OtherGovernmentSpending),
                results.Sum(r => r.EmigrantsThisMonth), results.Sum(r => r.ImmigrantsThisMonth),
                results.Last().RemainingTaxpayers, results.Average(r => r.UnemploymentRate),
                results.Last().PriceLevel, results.Last().InterestRate,
                results.Last().FirmsBySize.Values.Sum(), results.Average(r => r.TradeBalance)
            );
        }

        static void WriteEnhancedSummaryCsv(List<ScenarioSummary> summaries, string path)
        {
            using var sw = new StreamWriter(path);
            sw.WriteLine("Scenario,Months,UBI,NetFiscalPosition,PITScale,CorpRate,VATRate," +
                        "TotalTaxes,TotalUBI,TotalOtherSpending,TotalEmigrants,TotalImmigrants," +
                        "FinalTaxpayers,AvgUnemployment,FinalPriceLevel,FinalInterestRate," +
                        "FinalFirms,AvgTradeBalance");
            
            foreach (var s in summaries)
            {
                sw.WriteLine(string.Join(",",
                    s.Name, s.Months, s.UBI, s.Net, s.PitScale.ToString("F4"), s.CorpRate.ToString("F4"), s.VatRate.ToString("F4"),
                    s.TotalTaxes, s.TotalUBI, s.TotalOtherSpending, s.TotalEmigrants, s.TotalImmigrants,
                    s.FinalTaxpayers, s.AvgUnemployment.ToString("F4"), s.FinalPriceLevel.ToString("F4"), 
                    s.FinalInterestRate.ToString("F4"), s.FinalFirms, s.AvgTradeBalance
                ));
            }
        }

        // ==========================================
        // COMMAND LINE PARSING
        // ==========================================

        static decimal GetArg(string[] args, string name, decimal fallback)
        {
            var ix = Array.IndexOf(args, name);
            return ix >= 0 && ix + 1 < args.Length && decimal.TryParse(args[ix + 1], out var v) ? v : fallback;
        }

        static string? GetArgStr(string[] args, string name)
        {
            var ix = Array.IndexOf(args, name);
            return ix >= 0 && ix + 1 < args.Length ? args[ix + 1] : null;
        }

        static bool GetArgBool(string[] args, string name, bool fallback)
        {
            var ix = Array.IndexOf(args, name);
            if (ix >= 0 && ix + 1 < args.Length)
            {
                var s = args[ix + 1].Trim().ToLowerInvariant();
                return s switch
                {
                    "true" or "1" or "yes" or "y" => true,
                    "false" or "0" or "no" or "n" => false,
                    _ => fallback
                };
            }
            return fallback;
        }
    }
}