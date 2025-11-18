using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyAF.ModelGenerator;

/// <summary>
/// Automated model class generator that reads EasyPower CSV field definitions
/// and generates C# model classes with exact column name matching.
/// </summary>
public class ModelGenerator
{
    private readonly string _csvFilePath;
    private readonly string _outputDirectory;
    private readonly Dictionary<string, List<string>> _classColumns;
    
    // Map CSV class names to C# class names
    private readonly Dictionary<string, string> _classNameMap = new()
    {
        { "Arc Flash Scenario Report", "ArcFlash" },
        { "Equipment Duty Scenario Report", "ShortCircuit" },
        { "Buses", "Bus" },
        { "Panels", "Panel" },
        { "MCCs", "MCC" },
        { "Utilities", "Utility" },
        { "Generators", "Generator" },
        { "Cables", "Cable" },
        { "Busways", "Busway" },
        { "Transmission Lines", "TransmissionLine" },
        { "CL Reactors", "CLReactor" },
        { "2W Transformers", "Transformer" },
        { "3W Transformers", "Transformer3W" },
        { "Zigzag Transformers", "ZigzagTransformer" },
        { "LV Breakers", "LVCB" },
        { "HV Breakers", "HVBreaker" },
        { "Relays", "Relay" },
        { "CTs", "CT" },
        { "Fuses", "Fuse" },
        { "Switches", "Switch" },
        { "ATSs", "ATS" },
        { "Motors", "Motor" },
        { "Loads", "Load" },
        { "Shunts", "Shunt" },
        { "Capacitors", "Capacitor" },
        { "Filters", "Filter" },
        { "AFDs", "AFD" },
        { "UPSs", "UPS" },
        { "Inverters", "Inverter" },
        { "Rectifiers", "Rectifier" },
        { "Photovoltaics", "Photovoltaic" },
        { "Batteries", "Battery" },
        { "Meters", "Meter" },
        { "POCs", "POC" }
    };

    public ModelGenerator(string csvFilePath, string outputDirectory)
    {
        _csvFilePath = csvFilePath;
        _outputDirectory = outputDirectory;
        _classColumns = new Dictionary<string, List<string>>();
    }

    public void ParseCsvFile()
    {
        Console.WriteLine($"Parsing CSV file: {_csvFilePath}");
        
        var lines = File.ReadAllLines(_csvFilePath);
        
        foreach (var line in lines)
        {
            var columns = line.Split(',');
            var className = columns[0].Trim();
            
            if (string.IsNullOrWhiteSpace(className))
                continue;
            
            var columnList = columns.Skip(1)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim())
                .ToList();
            
            _classColumns[className] = columnList;
        }
        
        Console.WriteLine($"Parsed {_classColumns.Count} class definitions");
    }

    public void GenerateAllModels()
    {
        if (!Directory.Exists(_outputDirectory))
            Directory.CreateDirectory(_outputDirectory);
        
        var generated = 0;
        
        foreach (var kvp in _classColumns)
        {
            var csvClassName = kvp.Key;
            var columns = kvp.Value;
            
            if (!_classNameMap.TryGetValue(csvClassName, out var csharpClassName))
            {
                Console.WriteLine($"??  Skipping unmapped class: {csvClassName}");
                continue;
            }
            
            try
            {
                GenerateModelClass(csvClassName, csharpClassName, columns);
                Console.WriteLine($"? Generated {csharpClassName}.cs ({columns.Count} properties)");
                generated++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Failed to generate {csharpClassName}: {ex.Message}");
            }
        }
        
        Console.WriteLine($"\n?? Successfully generated {generated} model classes!");
    }

    private void GenerateModelClass(string csvClassName, string csharpClassName, List<string> columns)
    {
        var sb = new StringBuilder();
        
        // File header
        sb.AppendLine("using EasyAF.Data.Attributes;");
        sb.AppendLine();
        sb.AppendLine("namespace EasyAF.Data.Models;");
        sb.AppendLine();
        
        // Class summary
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Represents {GetArticle(csharpClassName)} {csharpClassName} with comprehensive properties from EasyPower exports.");
        sb.AppendLine("/// All properties are strings to preserve source data fidelity without premature parsing.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("/// <remarks>");
        sb.AppendLine("/// <para>");
        sb.AppendLine($"/// <strong>EasyPower Correlation:</strong> Maps to \"{csvClassName}\" class in EasyPower CSV exports.");
        sb.AppendLine("/// </para>");
        sb.AppendLine("/// <para>");
        sb.AppendLine($"/// <strong>Auto-Generated:</strong> This file was automatically generated from CSV field definitions.");
        sb.AppendLine($"/// Do not manually edit property names - regenerate from source CSV if changes are needed.");
        sb.AppendLine("/// </para>");
        sb.AppendLine("/// </remarks>");
        sb.AppendLine($"[EasyPowerClass(\"{csvClassName}\")]");
        sb.AppendLine($"public class {csharpClassName}");
        sb.AppendLine("{");
        
        // Generate properties
        var idPropertyName = GenerateProperties(sb, columns, csvClassName, csharpClassName);
        
        // Add Id alias if we have an ID property
        if (idPropertyName != null)
        {
            sb.AppendLine();
            sb.AppendLine("    /// <summary>Alias for " + idPropertyName + " (convenience property for dictionary indexing - not serialized).</summary>");
            sb.AppendLine("    [System.Text.Json.Serialization.JsonIgnore]");
            sb.AppendLine("    [Newtonsoft.Json.JsonIgnore]");
            sb.AppendLine("    public string? Id");
            sb.AppendLine("    {");
            sb.AppendLine($"        get => {idPropertyName};");
            sb.AppendLine($"        set => {idPropertyName} = value;");
            sb.AppendLine("    }");
        }
        
        // Constructor
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine($"    /// Initializes a new instance of the <see cref=\"{csharpClassName}\"/> class.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public {csharpClassName}() {{ }}");
        
        // ToString
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine($"    /// Returns a string representation of the {csharpClassName}.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public override string ToString()");
        sb.AppendLine("    {");
        if (idPropertyName != null)
        {
            sb.AppendLine($"        return $\"{csharpClassName}: {{{idPropertyName}}}\";");
        }
        else
        {
            sb.AppendLine($"        return \"{csharpClassName}\";");
        }
        sb.AppendLine("    }");
        
        sb.AppendLine("}");
        
        // Write to file
        var fileName = Path.Combine(_outputDirectory, $"{csharpClassName}.cs");
        File.WriteAllText(fileName, sb.ToString());
    }

    private string? GenerateProperties(StringBuilder sb, List<string> columns, string csvClassName, string csharpClassName)
    {
        string? idPropertyName = null;
        var firstProperty = true;
        
        foreach (var column in columns)
        {
            if (!firstProperty)
                sb.AppendLine();
            
            var propertyName = ConvertColumnNameToPropertyName(column);
            var category = InferCategory(column);
            var units = ExtractUnits(column);
            var isRequired = IsRequiredProperty(column, csvClassName);
            var description = GenerateDescription(column);
            
            // Track ID property (first column is usually the ID)
            if (firstProperty && IsIdentifierColumn(column, csvClassName))
            {
                idPropertyName = propertyName;
            }
            
            // Generate XML summary
            sb.AppendLine($"    /// <summary>{description} (Column: {column})</summary>");
            
            // Generate attributes
            sb.AppendLine($"    [Category(\"{category}\")]");
            sb.AppendLine($"    [Description(\"{description}\")]");
            
            if (isRequired)
                sb.AppendLine("    [Required]");
            
            if (!string.IsNullOrEmpty(units))
                sb.AppendLine($"    [Units(\"{units}\")]");
            
            // Generate property
            sb.AppendLine($"    public string? {propertyName} {{ get; set; }}");
            
            firstProperty = false;
        }
        
        return idPropertyName;
    }

    private string ConvertColumnNameToPropertyName(string columnName)
    {
        // Remove parentheses and their contents
        var cleaned = Regex.Replace(columnName, @"\([^)]*\)", "").Trim();
        
        // Remove special characters: spaces, slashes, dashes, periods, apostrophes
        cleaned = cleaned.Replace(" ", "")
                        .Replace("/", "")
                        .Replace("-", "")
                        .Replace(".", "")
                        .Replace("'", "")
                        .Replace("#", "Number");
        
        // Handle special cases
        if (cleaned == "2WTransformers") cleaned = "Transformers2W";
        if (cleaned == "3WTransformers") cleaned = "Transformers3W";
        
        // Ensure first character is uppercase
        if (cleaned.Length > 0 && char.IsLower(cleaned[0]))
            cleaned = char.ToUpper(cleaned[0]) + cleaned.Substring(1);
        
        return cleaned;
    }

    private string InferCategory(string columnName)
    {
        var lower = columnName.ToLower();
        
        if (lower.Contains("failure") || lower.Contains("repair") || lower.Contains("replace") || 
            lower.Contains("reliability") || lower.Contains("downtime"))
            return "Reliability";
        
        if (lower.Contains("comment") || lower.Contains("data status") || lower.Contains("description"))
            return "Metadata";
        
        if (lower.Contains("kv") || lower.Contains("kva") || lower.Contains("kw") || lower.Contains("amp") ||
            lower.Contains("voltage") || lower.Contains("current") || lower.Contains("phase") || 
            lower.Contains("impedance") || lower.Contains("fault") || lower.Contains("sc "))
            return "Electrical";
        
        if (lower.Contains("mfr") || lower.Contains("manufacturer") || lower.Contains("type") || 
            lower.Contains("style") || lower.Contains("model") || lower.Contains("rating") || 
            lower.Contains("frame") || lower.Contains("size"))
            return "Physical";
        
        if (lower.Contains("setting") || lower.Contains("control") || lower.Contains("tap") || 
            lower.Contains("ltc") || lower.Contains("regulation"))
            return "Control";
        
        if (lower.Contains("protection") || lower.Contains("trip") || lower.Contains("pickup") || 
            lower.Contains("delay") || lower.Contains("ground") || lower.Contains("inst") || 
            lower.Contains("st ") || lower.Contains("lt ") || lower.Contains("tcc"))
            return "Protection";
        
        if (lower.Contains("location") || lower.Contains("facility") || lower.Contains("area") || 
            lower.Contains("zone") || lower.Contains("floor"))
            return "Location";
        
        if (lower.Contains("demand") || lower.Contains("load") || lower.Contains("diversity"))
            return "Demand";
        
        if (lower == columns.First() || lower.Contains("status") || lower.Contains("bus"))
            return "Identity";
        
        return "General";
    }

    private string ExtractUnits(string columnName)
    {
        // Extract units from parentheses
        var match = Regex.Match(columnName, @"\(([^)]+)\)");
        if (match.Success)
        {
            var unit = match.Groups[1].Value;
            
            // Normalize common units
            if (unit == "A") return "A";
            if (unit.Contains("kA")) return "kA";
            if (unit.Contains("kV")) return "kV";
            if (unit.Contains("kW")) return "kW";
            if (unit.Contains("kVA")) return "kVA";
            if (unit.Contains("kVAR")) return "kVAR";
            if (unit == "h") return "h";
            if (unit.Contains("year")) return "/year";
            if (unit == "%") return "%";
            if (unit.Contains("sec")) return "s";
            if (unit.Contains("mm")) return "mm";
            if (unit.Contains("in") || unit.Contains("inches")) return "in";
            if (unit.Contains("cal/cm")) return "cal/cm²";
            if (unit.Contains("deg") || unit.Contains("C")) return "°C";
            if (unit.Contains("HP")) return "HP";
            if (unit.Contains("RPM")) return "RPM";
            if (unit.Contains("MVA")) return "MVA";
            if (unit.Contains("Ohm") || unit == "?") return "?";
            if (unit == "V") return "V";
            if (unit == "$") return "$";
            if (unit.Contains("$/h")) return "$/h";
            
            return unit;
        }
        
        return string.Empty;
    }

    private bool IsRequiredProperty(string columnName, string csvClassName)
    {
        var lower = columnName.ToLower();
        
        // First column (ID) is always required
        if (columnName == _classColumns[csvClassName].First())
            return true;
        
        // Key identifier properties
        if (lower.Contains("on bus") || lower.Contains("bus name"))
            return true;
        
        if (lower.Contains("scenario") && csvClassName.Contains("Report"))
            return true;
        
        // Rating properties
        if ((lower.Contains("mva") || lower.Contains("hp") || lower.Contains("kva")) && 
            !lower.Contains("demand") && !lower.Contains("dn "))
            return true;
        
        return false;
    }

    private string GenerateDescription(string columnName)
    {
        // Remove units from description
        var desc = Regex.Replace(columnName, @"\([^)]*\)", "").Trim();
        return desc;
    }

    private bool IsIdentifierColumn(string columnName, string csvClassName)
    {
        // First column is the identifier
        return columnName == _classColumns[csvClassName].First();
    }

    private string GetArticle(string className)
    {
        var vowels = new[] { 'A', 'E', 'I', 'O', 'U' };
        return vowels.Contains(className[0]) ? "an" : "a";
    }

    private List<string> columns => _classColumns.Values.First();
}

/// <summary>
/// Console application entry point for model generator.
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== EasyAF Model Generator ===\n");
        
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: EasyAF.ModelGenerator <csv-file-path> <output-directory>");
            Console.WriteLine("\nExample:");
            Console.WriteLine("  EasyAF.ModelGenerator \"C:\\src\\EasyAFv3\\easypower fields.csv\" \"C:\\src\\EasyAFv3\\lib\\EasyAF.Data\\Models\"");
            return;
        }
        
        var csvPath = args[0];
        var outputDir = args[1];
        
        if (!File.Exists(csvPath))
        {
            Console.WriteLine($"? ERROR: CSV file not found: {csvPath}");
            return;
        }
        
        try
        {
            var generator = new ModelGenerator(csvPath, outputDir);
            generator.ParseCsvFile();
            generator.GenerateAllModels();
            
            Console.WriteLine("\n? Model generation complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n? ERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}
