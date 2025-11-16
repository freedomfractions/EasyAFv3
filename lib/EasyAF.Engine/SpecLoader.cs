using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using EasyAF.Data; // for TableFormattingOptions
using EasyAF.Data.Models;

namespace EasyAF.Engine
{
    public class SpecLoadResult
    {
        public List<TableDefinition> Tables { get; } = new();
        public List<string> Warnings { get; } = new();
        public List<string> Errors { get; } = new();
        public List<PropertyMappingSpec>? PropertyMappings { get; set; }
        public string? SpecVersion { get; set; }
        public string? SpecChecksum { get; set; }
        public bool HasErrors => Errors.Count > 0;
    }

    public static class SpecLoader
    {
        private static readonly HashSet<string> AllowedFilterOperators = new(StringComparer.OrdinalIgnoreCase)
        { "eq","neq","lt","lte","gt","gte","contains" };
        private static readonly Regex PascalCaseRegex = new("^[A-Z][A-Za-z0-9]*$");
        private static readonly HashSet<string> PascalCaseAllowList = new(StringComparer.OrdinalIgnoreCase)
        {
            // allowlist for any special or legacy names if needed (currently none)
        };

        public static SpecLoadResult LoadFromJson(string json)
        {
            var result = new SpecLoadResult();

            // 1. PascalCase enforcement pass (lightweight DOM walk)
            try
            {
                using var doc = JsonDocument.Parse(json);
                var invalid = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                Walk(doc.RootElement, invalid);
                if (invalid.Count > 0)
                {
                    foreach (var name in invalid.OrderBy(s => s))
                        result.Errors.Add($"Invalid JSON field name '{name}' (must be PascalCase – e.g., 'FieldName').");
                    return result; // fail fast
                }
            }
            catch (JsonException jx)
            {
                result.Errors.Add($"JSON parse error (pre-validate): {jx.Message}");
                return result;
            }

            TableSpec[]? specs = null;
            try
            {
                // attempt root object first
                var root = JsonSerializer.Deserialize<SpecFileRoot>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (root != null && root.Tables != null && root.Tables.Length > 0)
                {
                    specs = root.Tables;
                    result.PropertyMappings = root.PropertyMappings?.ToList();
                    result.SpecVersion = root.SpecVersion;
                    result.SpecChecksum = root.SpecChecksum;
                }
                else
                {
                    specs = JsonSerializer.Deserialize<TableSpec[]>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"JSON parse error: {ex.Message}");
                return result;
            }
            if (specs == null) return result;

            // Validate duplicate table IDs
            var dupIds = specs.GroupBy(s => s.Id, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            foreach (var d in dupIds) result.Errors.Add($"Duplicate table Id '{d}' in spec.");
            if (result.HasErrors) return result;

            foreach (var s in specs)
            {
                var td = new TableDefinition(s.Id, s.AltText)
                {
                    EmptyMessage = string.IsNullOrWhiteSpace(s.EmptyMessage) ? "No entries." : s.EmptyMessage!.Trim()
                };
                if (s.EmptyFormatting != null) td.EmptyFormatting = s.EmptyFormatting;

                if (!string.IsNullOrWhiteSpace(s.Mode))
                {
                    if (string.Equals(s.Mode, "diff", StringComparison.OrdinalIgnoreCase)) td.Mode = TableMode.Diff;
                    else if (!string.Equals(s.Mode, "new", StringComparison.OrdinalIgnoreCase))
                        result.Warnings.Add($"Table '{s.Id}': unknown Mode '{s.Mode}' (defaulting to 'new').");
                }
                if (s.HideIfNoDiff.HasValue) td.HideIfNoDiff = s.HideIfNoDiff.Value;

                td.Formatting ??= new TableFormattingOptions
                {
                    FontName = "Arial",
                    FontSizeHalfPoints = 20,
                    HorizontalAlignment = "left",
                    VerticalAlignment = "center",
                    HeaderFillColor = "FFFFFF",
                    AlternateRowFillColor = "C1E4F5"
                };
                if (s.AllowRowBreakAcrossPages.HasValue)
                    td.Formatting.AllowRowBreakAcrossPages = s.AllowRowBreakAcrossPages.Value;
                if (s.Formatting != null)
                {
                    if (!string.IsNullOrWhiteSpace(s.Formatting.FontName)) td.Formatting.FontName = s.Formatting.FontName.Trim();
                    if (s.Formatting.FontSize.HasValue && s.Formatting.FontSize.Value >0) td.Formatting.FontSizeHalfPoints = (int)Math.Round(s.Formatting.FontSize.Value *2);
                    if (!string.IsNullOrWhiteSpace(s.Formatting.HorizontalAlignment)) td.Formatting.HorizontalAlignment = s.Formatting.HorizontalAlignment;
                    if (!string.IsNullOrWhiteSpace(s.Formatting.VerticalAlignment)) td.Formatting.VerticalAlignment = s.Formatting.VerticalAlignment;
                    if (!string.IsNullOrWhiteSpace(s.Formatting.HeaderFill)) td.Formatting.HeaderFillColor = s.Formatting.HeaderFill.Trim();
                    if (!string.IsNullOrWhiteSpace(s.Formatting.AlternateRowFill)) td.Formatting.AlternateRowFillColor = s.Formatting.AlternateRowFill.Trim();
                    if (s.Formatting.RemoveDuplicateLines.HasValue) td.Formatting.RemoveDuplicateLines = s.Formatting.RemoveDuplicateLines.Value;
                    if (s.Formatting.RemoveDuplicateLinesStrict.HasValue) td.Formatting.RemoveDuplicateLinesStrict = s.Formatting.RemoveDuplicateLinesStrict.Value;
                    if (s.Formatting.FitToWindow.HasValue) td.Formatting.FitToWindow = s.Formatting.FitToWindow.Value;
                    if (s.Formatting.MergeBreakColumns != null && s.Formatting.MergeBreakColumns.Length >0)
                        td.Formatting.MergeBreakColumnIndexes = s.Formatting.MergeBreakColumns.Select(i => i -1).Where(i => i >=0).Distinct().ToList();
                    if (s.Formatting.MergeBreakColumnHeaders != null && s.Formatting.MergeBreakColumnHeaders.Length >0 && s.Columns != null && s.Columns.Length >0)
                    {
                        var headerMap = s.Columns.Select((c, idx) => new { c.Header, idx }).ToDictionary(x => x.Header.Trim(), x => x.idx, StringComparer.OrdinalIgnoreCase);
                        var list = new List<int>();
                        foreach (var h in s.Formatting.MergeBreakColumnHeaders)
                        {
                            if (string.IsNullOrWhiteSpace(h)) continue;
                            if (headerMap.TryGetValue(h.Trim(), out var colIdx)) list.Add(colIdx);
                        }
                        if (list.Count >0) td.Formatting.MergeBreakColumnIndexes ??= list.Distinct().OrderBy(i => i).ToList(); else result.Warnings.Add($"Table '{s.Id}': none of MergeBreakColumnHeaders matched.");
                    }
                }

                if (s.SortSpecs?.Length > 0) td.SortSpecs = s.SortSpecs.ToList();

                if (s.FilterSpecs?.Length > 0)
                {
                    foreach (var f in s.FilterSpecs)
                    {
                        if (!AllowedFilterOperators.Contains(f.Operator))
                        {
                            result.Warnings.Add($"Table '{s.Id}': filter operator '{f.Operator}' not recognized. Defaulting to 'eq'.");
                            f.Operator = "eq";
                        }
                        if (!string.IsNullOrWhiteSpace(f.RightPropertyPath) && !string.IsNullOrWhiteSpace(f.Value))
                            result.Warnings.Add($"Table '{s.Id}': filter has both Value and RightPropertyPath for '{f.PropertyPath}'. Value ignored.");
                    }
                    td.FilterSpecs = s.FilterSpecs.ToList();
                }

                if (s.FilterGroups?.Length > 0)
                {
                    td.FilterGroups = new List<FilterGroup>();
                    foreach (var fg in s.FilterGroups)
                    {
                        var grp = new FilterGroup { Logic = fg.Logic ?? "AND" };
                        foreach (var f in fg.Filters)
                        {
                            if (!AllowedFilterOperators.Contains(f.Operator))
                            {
                                result.Warnings.Add($"Table '{s.Id}': group filter operator '{f.Operator}' not recognized. Defaulting to 'eq'.");
                                f.Operator = "eq";
                            }
                            grp.Filters.Add(f);
                        }
                        if (grp.Filters.Count > 0) td.FilterGroups.Add(grp);
                    }
                }

                if (s.RowConditions?.Length > 0) td.RowConditions = new List<RowConditionSpec>(s.RowConditions);
                if (s.GlobalCellConditions?.Length > 0) td.GlobalCellConditions = new List<CellConditionSpec>(s.GlobalCellConditions);

                var seenHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                double widthSum = 0; int widthSpecified = 0;
                foreach (var col in s.Columns)
                {
                    if (!seenHeaders.Add(col.Header)) result.Warnings.Add($"Table '{s.Id}': duplicate column header '{col.Header}'.");
                    if (col.WidthPercent.HasValue)
                    {
                        var w = col.WidthPercent.Value;
                        if (w <= 0 || w > 100) result.Warnings.Add($"Table '{s.Id}': column '{col.Header}' width {w} out of range (0-100].");
                        else { widthSum += w; widthSpecified++; }
                    }
                }
                if (widthSpecified > 0 && widthSum > 100.0001)
                    result.Warnings.Add($"Table '{s.Id}': specified column widths sum to {widthSum:0.##} > 100.");

                if (s.Columns.Length > 0)
                {
                    td.Formatting.ColumnFontNames = new List<string?>(new string?[s.Columns.Length]);
                    td.Formatting.ColumnFontSizeHalfPoints = new List<int?>(new int?[s.Columns.Length]);
                    td.Formatting.ColumnHorizontalAlignments = new List<string?>(new string?[s.Columns.Length]);
                    td.Formatting.ColumnVerticalAlignments = new List<string?>(new string?[s.Columns.Length]);
                }

                for (int ci = 0; ci < s.Columns.Length; ci++)
                {
                    var col = s.Columns[ci];
                    var cd = new ColumnDefinition(col.Header) { Name = col.Name, MergeVertically = col.MergeVertically };
                    if (col.Conditions?.Length > 0) cd.Conditions = new List<CellConditionSpec>(col.Conditions);

                    if (!string.IsNullOrWhiteSpace(col.Literal))
                        cd.Renderer = (ctx, o) => col.Literal!;
                    else if (!string.IsNullOrWhiteSpace(col.Expression))
                    {
                        var compiled = ExpressionCompiler.GetOrAdd(col.Expression!, col.NumberFormat);
                        cd.Renderer = (ctx, o) => o == null ? string.Empty : compiled.Render(o);
                        cd.IsExpression = true; cd.ExpressionSource = col.Expression; cd.ExpressionNumberFormat = col.NumberFormat;
                    }
                    else if (!string.IsNullOrWhiteSpace(col.Format))
                        cd.Renderer = (ctx, o) => o == null ? string.Empty : EasyAFEngine.CleanOutput(EasyAFEngine.RenderFormat(o, col.Format!));
                    else if (col.PropertyPaths?.Length > 0)
                        cd.Renderer = (ctx, o) =>
                        {
                            if (o == null) return string.Empty;
                            var vals = new List<string>();
                            foreach (var path in col.PropertyPaths)
                            {
                                var v = EasyAFEngine.EvaluatePropertyPath(o, path);
                                if (!string.IsNullOrWhiteSpace(v)) vals.Add(v);
                            }
                            return string.Join(col.JoinWith ?? "\n", vals);
                        };

                    td.Columns.Add(cd);

                    if (!string.IsNullOrWhiteSpace(col.FontName)) td.Formatting.ColumnFontNames![ci] = col.FontName.Trim();
                    if (col.FontSize.HasValue && col.FontSize.Value > 0) td.Formatting.ColumnFontSizeHalfPoints![ci] = (int)Math.Round(col.FontSize.Value * 2);
                    if (!string.IsNullOrWhiteSpace(col.HorizontalAlignment)) td.Formatting.ColumnHorizontalAlignments![ci] = col.HorizontalAlignment;
                    if (!string.IsNullOrWhiteSpace(col.VerticalAlignment)) td.Formatting.ColumnVerticalAlignments![ci] = col.VerticalAlignment;
                }

                var widths = s.Columns.Select(c => c.WidthPercent).ToList();
                if (widths.Any(w => w.HasValue))
                {
                    double specifiedTotal = widths.Where(w => w.HasValue).Sum(w => w!.Value);
                    int unspecifiedCount = widths.Count(w => !w.HasValue);
                    double remaining = Math.Max(0, 100 - specifiedTotal);
                    double autoEach = unspecifiedCount > 0 ? remaining / unspecifiedCount : 0;
                    var final = widths.Select(w => w ?? autoEach).ToList();
                    td.Formatting.ColumnWidthPercents = final;
                }

                result.Tables.Add(td);
            }

            if (!string.IsNullOrWhiteSpace(result.SpecChecksum))
            {
                try
                {
                    using var sha = System.Security.Cryptography.SHA256.Create();
                    var basis = string.Join("|", specs.Select(s => s.Id + ":" + string.Join(",", s.Columns.Select(c => c.Header))));
                    var hash = BitConverter.ToString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(basis))).Replace("-", string.Empty);
                    if (!hash.Equals(result.SpecChecksum, StringComparison.OrdinalIgnoreCase))
                        result.Warnings.Add("Spec checksum mismatch (computed does not match provided SpecChecksum).");
                }
                catch (Exception ex)
                {
                    result.Warnings.Add("Checksum validation failed: " + ex.Message);
                }
            }

            return result;
        }

        private static void Walk(JsonElement el, HashSet<string> invalid)
        {
            switch (el.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var prop in el.EnumerateObject())
                    {
                        var name = prop.Name;
                        if (!PascalCaseAllowList.Contains(name) && !PascalCaseRegex.IsMatch(name))
                        {
                            // allow leading $ for potential future schema declarations without failing
                            if (!name.StartsWith("$")) invalid.Add(name);
                        }
                        Walk(prop.Value, invalid);
                    }
                    break;
                case JsonValueKind.Array:
                    foreach (var item in el.EnumerateArray()) Walk(item, invalid);
                    break;
            }
        }

        private static string EvalExpression(object rowObj, string expr, string? numberFormat)
        {
            var tokenMatches = System.Text.RegularExpressions.Regex.Matches(expr, "{([^}]+)}");
            var work = expr;
            foreach (System.Text.RegularExpressions.Match m in tokenMatches)
            {
                var token = m.Groups[1].Value;
                var raw = EasyAFEngine.EvaluatePropertyPath(rowObj, token);
                if (!double.TryParse(raw, out var d)) d = 0;
                work = work.Replace(m.Value, d.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            try
            {
                var value = SimpleExpressionEvaluator.Evaluate(work);
                if (numberFormat != null && double.IsFinite(value)) return value.ToString(numberFormat);
                return value.ToString("0.###");
            }
            catch { return string.Empty; }
        }
    }
}
