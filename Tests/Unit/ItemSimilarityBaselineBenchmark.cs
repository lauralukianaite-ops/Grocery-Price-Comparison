using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using backend.Entities;
using backend.Services;
using Xunit;
using Xunit.Abstractions;

namespace backend.Tests;

public class ItemSimilarityBenchmark
{
    private readonly ITestOutputHelper _output;
    private static readonly string FilePath = "benchmark-results.txt";
    private static readonly object FileLock = new object();

    public ItemSimilarityBenchmark(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [InlineData(1_000, "Pienas")]
    [InlineData(10_000, "Pienas")]
    [InlineData(50_000, "Pienas")]
    [InlineData(50_000, "Prancūziškas česnakinis batonas")]
    [InlineData(50_000, "Televizorius Samsung")]
    public void Benchmark_FindSimilarItems_ExecutionTime(int itemCount, string searchQuery)
    {
        const double threshold = 0.3;
        const int iterations = 10;

        var items = GenerateTestItems(itemCount);

        var index = new ItemSimilarityIndex();
        index.BuildOrUpdateIndex(items);

        for (int i = 0; i < 3; i++)
        {
            index.Find(searchQuery, threshold);
        }

        var executionTimes = new List<double>();
        int foundCount = 0;

        for (int i = 0; i < iterations; i++)
        {
            var sw = Stopwatch.StartNew();
            var results = index.Find(searchQuery, threshold);
            sw.Stop();

            executionTimes.Add(sw.Elapsed.TotalMilliseconds);
            foundCount = results.Count;
        }

        double avgTime = executionTimes.Average();
        double minTime = executionTimes.Min();
        double maxTime = executionTimes.Max();

        _output.WriteLine($"QUERY: \"{searchQuery}\" | ITEMS: {itemCount:N0} | Avg: {avgTime:F2} ms");

        WriteResultToFile(searchQuery, itemCount, foundCount, avgTime, minTime, maxTime);

        Assert.True(executionTimes.Count == iterations);
    }

    private static void WriteResultToFile(string query, int count, int found, double avg, double min, double max)
    {
        lock (FileLock)
        {
            if (!File.Exists(FilePath))
            {
                string header = string.Format(
                    "{0,-19} | {1,-32} | {2,-10} | {3,-8} | {4,-10} | {5,-10} | {6,-10}\n{7}",
                    "Date and Time", "Query", "Item Count", "Matches", "Avg Time", "Min Time", "Max Time",
                    new string('-', 115) + "\n"
                );
                File.WriteAllText(FilePath, header);
            }

            string formattedLine = string.Format(
                "{0,-19} | {1,-32} | {2,10:N0} | {3,8:N0} | {4,8:F2} ms | {5,8:F2} ms | {6,8:F2} ms",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                query.Length > 32 ? query.Substring(0, 29) + "..." : query,
                count,
                found,
                avg,
                min,
                max
            );

            File.AppendAllLines(FilePath, new[] { formattedLine });
        }
    }

    private static List<Item> GenerateTestItems(int count)
    {
        var rnd = new Random(42);

        var words = new[]
        {
            "pienas", "varškė", "sūris", "duona", "batonas", "česnakinis", "prancūziškas", "sultys",
            "apelsinų", "obuolių", "kefyras", "jogurtas", "sviestas", "kiaušiniai", "vištiena", "kiauliena",
            "žuvis", "lašiša", "makaronai", "ryžiai", "grikiai", "aliejus", "cukrus", "druska", "miltai",
            "šokoladas", "sausainiai", "kava", "arbata", "vanduo", "alus", "vynas", "bulvės", "morkos",
            "agurkai", "pomidorai", "bananai", "citrinos", "šviežias", "rūkytas", "ekologiškas",
            "Dvaro", "Rokiškio", "Žemaitijos", "Vičiūnai", "Pieno žvaigždės", "Barbora", "Premium"
        };

        var stores = new[] { "Maxima", "Iki", "Rimi", "Lidl", "Norfa" };
        var items = new List<Item>(count);

        for (int i = 1; i <= count; i++)
        {
            int wordCount = rnd.Next(2, 5);
            var selectedWords = Enumerable.Range(0, wordCount).Select(_ => words[rnd.Next(words.Length)]);
            string name = $"{string.Join(" ", selectedWords)} {rnd.Next(1, 10) * 100}g";

            items.Add(new Item
            {
                Id = i,
                Name = name,
                Prices = new List<Price>
                {
                    new Price
                    {
                        Cost = (decimal)(rnd.NextDouble() * 5 + 0.5),
                        RecordedAt = DateTime.Now,
                        Store = new Store { Name = stores[rnd.Next(stores.Length)] }
                    }
                }
            });
        }

        return items;
    }
}