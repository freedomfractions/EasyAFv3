using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace EasyAF.CsvParser;

/// <summary>
/// Parses the EasyPower fields CSV to extract column headers for each equipment class.
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        var csvPath = args.Length > 0 ? args[0] : "easypower fields.csv";
        
        if (!File.Exists(csvPath))
        {
            Console.WriteLine($"ERROR: File not found: {csvPath}");
            return;
        }

        Console.WriteLine($"Parsing: {csvPath}\n");

        // Our 6 core classes
        var targetClasses = new Dictionary<string, string>
        {
            { "Buses", "Bus" },
            { "LV Breakers", "LVCB" },
            { "Cables", "Cable" },
            { "Fuses", "Fuse" },
            { "Arc Flash Scenario Report", "ArcFlash" },
            { "Equipment Duty Scenario Report", "ShortCircuit" }
        };

        var results = new Dictionary<string, ClassInfo>();
        var lines = File.ReadAllLines(csvPath);

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var fields = line.Split(',').Select(f => f.Trim()).ToArray();
            var className = fields[0];

            if (targetClasses.ContainsKey(className))
            {
                Console.WriteLine($"Found: {className}");
                
                // Collect all fields from this line
                var allFields = new List<string>(fields);

                // Check for continuation lines (start with comma)
                int j = i + 1;
                while (j < lines.Length && lines[j].StartsWith(","))
                {
                    var contFields = lines[j].Split(',').Select(f => f.Trim()).ToArray();
                    allFields.AddRange(contFields);
                    j++;
                }

                // Filter: skip first field (class name), remove empties
                var columns = allFields.Skip(1).Where(f => !string.IsNullOrWhiteSpace(f)).ToList();

                results[className] = new ClassInfo
                {
                    EasyPowerName = className,
                    EasyAFClassName = targetClasses[className],
                    Columns = columns
                };

                Console.WriteLine($"  -> {targetClasses[className]}: {columns.Count} columns\n");
            }
        }

        // Save to JSON
        var json = JsonSerializer.Serialize(results, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        File.WriteAllText("parsed-easypower-classes.json", json);
        Console.WriteLine($"\nResults saved to: parsed-easypower-classes.json");

        // Display summary
        Console.WriteLine("\n=== SUMMARY ===");
        foreach (var kvp in results.OrderBy(r => r.Value.EasyAFClassName))
        {
            Console.WriteLine($"{kvp.Value.EasyAFClassName,-15} ({kvp.Key,-35}): {kvp.Value.Columns.Count,3} properties");
        }
    }
}

class ClassInfo
{
    public string EasyPowerName { get; set; } = "";
    public string EasyAFClassName { get; set; } = "";
    public List<string> Columns { get; set; } = new();
}
