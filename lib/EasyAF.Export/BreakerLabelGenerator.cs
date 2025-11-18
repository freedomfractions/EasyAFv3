using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.IO.Compression;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml; // provides OpenXmlElement
using EasyAF.Data.Models;

namespace EasyAF.Export
{
    public static class BreakerLabelGenerator
    {
        private static readonly Dictionary<string, PropertyInfo> LvcbProps = typeof(LVCB)
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);
        
        private static readonly Dictionary<string, PropertyInfo> ProjectProps = typeof(Project)
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

        // Units (presentation only) - updated to use LVCB.TripUnitXxx property names
        private static readonly Dictionary<string, string> UnitSuffixes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["LVCB.FrameSize"] = "A",
            ["LVCB.FrameRating"] = "A",
            ["LVCB.InterruptRating"] = "kA",
            ["LVCB.TripUnitTripPlug"] = "A",
            ["LVCB.TripUnitLtpuAmps"] = "A",
            ["LVCB.TripUnitStpuAmps"] = "A",
            ["LVCB.TripUnitInstAmps"] = "A",
            ["LVCB.TripUnitMaintAmps"] = "A",
            ["LVCB.TripUnitGfpuAmps"] = "A",
            // Legacy TripUnit.* tags (for backward compatibility with old templates)
            ["TripUnit.TripPlug"] = "A",
            ["TripUnit.LtpuAmps"] = "A",
            ["TripUnit.StpuAmps"] = "A",
            ["TripUnit.InstAmps"] = "A",
            ["TripUnit.MaintAmps"] = "A",
            ["TripUnit.GfpuAmps"] = "A"
        };

        // Fields that are considered strictly indicative of adjustability (heuristic Option A)
        // Now uses LVCB property names (flattened TripUnitXxx)
        private static readonly string[] StrictIndicatorPropertyNames = new[]
        {
            "TripUnitLtpuMult", "TripUnitLtdBand", "TripUnitLtdCurve",
            "TripUnitStpu", "TripUnitStdBand", "TripUnitStdI2t", "TripUnitStpuI2t",
            "TripUnitInstAmps", "TripUnitTripAdjust", "TripUnitInst",
            "TripUnitMaintSetting", "TripUnitGfpuAmps", "TripUnitGfd", "TripUnitGfdI2t"
        };
        private static readonly HashSet<string> StrictIndicatorSet = new(StrictIndicatorPropertyNames, StringComparer.OrdinalIgnoreCase);

        private static readonly string[] AlwaysIgnoreIndicatorNames = new[]
        {
            "TripUnitLtpuAmps", "TripUnitStpuAmps", "TripUnitMaintAmps", "TripUnitTripPlug", "TripUnitGfpu"
        };
        private static readonly HashSet<string> IgnoreIndicatorSet = new(AlwaysIgnoreIndicatorNames, StringComparer.OrdinalIgnoreCase);

        /// <summary>Result/summary of label generation.</summary>
        public sealed class BreakerLabelGenerationResult
        {
            public int BreakersProcessed { get; set; }
            public int PrototypeBlocks { get; set; }
            public int TagsReplaced { get; set; }
            public List<string> UnknownTags { get; set; } = new();
            public List<string> MissingMappedValues { get; set; } = new();
            // Map of Breaker.Id (or generated index) -> indicators that triggered adjustability (only populated when adjustableOnly filtering is applied)
            public Dictionary<string, List<string>> AdjustableTriggerMap { get; set; } = new();
        }

        /// <summary>
        /// Generate a labels document for the provided breakers using the embedded template (e.g. "Breaker.dotx").
        /// </summary>
        /// <param name="project">Optional project to supply Project.* tags.</param>
        /// <param name="breakers">Breakers to include.</param>
        /// <param name="templateName">Template file name (embedded resource lookup, case-insensitive).</param>
        /// <param name="outputPath">Destination .docx path.</param>
        /// <param name="adjustableOnly">If true include only breakers with adjustable trip units (auto inferred from settings).</param>
        /// <param name="clearUnmatchedTags">If true clears (empties) any SDT whose tag cannot be resolved; otherwise leaves placeholder text.</param>
        public static BreakerLabelGenerationResult GenerateBreakerLabels(Project? project, IEnumerable<LVCB> breakers, string templateName, string outputPath, bool adjustableOnly = true, bool clearUnmatchedTags = true)
        {
            if (breakers == null) throw new ArgumentNullException(nameof(breakers));
            var result = new BreakerLabelGenerationResult();
            var all = breakers.ToList();
            List<LVCB> filtered;
            if (adjustableOnly)
            {
                filtered = new List<LVCB>();
                foreach (var b in all)
                {
                    if (b == null) continue;
                    if (IsEffectivelyAdjustable(b, out var indicators))
                    {
                        filtered.Add(b);
                        var key = b.Id ?? ($"IDX_{filtered.Count}");
                        result.AdjustableTriggerMap[key] = indicators;
                    }
                }
            }
            else filtered = all.Where(b=>b!=null).ToList()!;

            if (filtered.Count == 0)
                throw new InvalidOperationException("No breakers matched criteria (adjustableOnly filter may have excluded all).");
            result.BreakersProcessed = filtered.Count;

            PrepareOutputFromTemplate(templateName, outputPath);

            using var doc = WordprocessingDocument.Open(outputPath, true);
            var main = doc.MainDocumentPart?.Document ?? throw new InvalidOperationException("Template main document part missing.");
            var body = main.Body ?? throw new InvalidOperationException("Template body missing.");

            var prototypeBlocks = body.ChildElements
                .Where(e => (e is Paragraph || e is Table) && e.Descendants<SdtElement>().Any(s => IsKnownTag(GetTag(s))))
                .ToList();
            if (prototypeBlocks.Count == 0)
                throw new InvalidOperationException("Template does not contain any SDT content controls with LVCB./TripUnit./Project. tags.");
            result.PrototypeBlocks = prototypeBlocks.Count;

            result.TagsReplaced += ReplaceTagsForBreaker(project, filtered[0], prototypeBlocks, result, clearUnmatchedTags);
            for (int i = 1; i < filtered.Count; i++)
            {
                var clones = prototypeBlocks.Select(b => (OpenXmlElement)b.CloneNode(true)).ToList();
                foreach (var c in clones) body.Append(c);
                result.TagsReplaced += ReplaceTagsForBreaker(project, filtered[i], clones, result, clearUnmatchedTags);
            }
            main.Save();
            return result;
        }

        /// <summary>
        /// Overload for backward compatibility (no Project metadata tags).
        /// </summary>
        public static BreakerLabelGenerationResult GenerateBreakerLabels(IEnumerable<LVCB> breakers, string templateName, string outputPath, bool adjustableOnly = true)
            => GenerateBreakerLabels(null, breakers, templateName, outputPath, adjustableOnly, clearUnmatchedTags: true);

        /// <summary>
        /// Determines if a breaker has an adjustable trip unit based on heuristics (Option A).
        /// Checks TripUnitAdjustable flag and presence of meaningful trip unit settings.
        /// </summary>
        /// <param name="lvcb">The breaker to check.</param>
        /// <param name="indicators">Output list of property names that indicate adjustability.</param>
        /// <returns>True if the breaker is considered adjustable, otherwise false.</returns>
        public static bool IsEffectivelyAdjustable(LVCB lvcb, out List<string> indicators)
        {
            indicators = new();
            if (lvcb == null) return false;

            // Check explicit adjustable flag (if it's a boolean string like "True"/"False" or actual boolean)
            bool flagged = false;
            if (!string.IsNullOrWhiteSpace(lvcb.TripUnitAdjustable))
            {
                var adj = lvcb.TripUnitAdjustable.Trim().ToLowerInvariant();
                flagged = adj is "true" or "t" or "yes" or "y" or "1" or "x" or "adj" or "adjustable";
            }

            // Collect strict indicators regardless of current flag
            foreach (var name in StrictIndicatorPropertyNames)
            {
                var prop = typeof(LVCB).GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop == null) continue;
                var valObj = prop.GetValue(lvcb);
                if (valObj is string s && IsMeaningfulValue(s))
                {
                    // special handling: Inst counted only if not literal "fixed"
                    if (string.Equals(name, "TripUnitInst", StringComparison.OrdinalIgnoreCase) && string.Equals(s.Trim(), "fixed", StringComparison.OrdinalIgnoreCase))
                        continue;
                    indicators.Add(name);
                }
            }

            // Determine effective adjustability: explicit flag OR indicators present
            bool indicated = indicators.Count > 0;
            if (!(flagged || indicated)) return false;

            // If Inst is fixed AND only indicator is InstAmps -> treat as not adjustable
            if (string.Equals(lvcb.TripUnitInst?.Trim(), "fixed", StringComparison.OrdinalIgnoreCase) && 
                indicators.Count == 1 && 
                indicators[0].Equals("TripUnitInstAmps", StringComparison.OrdinalIgnoreCase))
            {
                indicators.Clear();
                return false;
            }
            return true;
        }

        private static bool IsMeaningfulValue(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            var t = s.Trim();
            if (t.Length == 0) return false;
            if (string.Equals(t, "fixed", StringComparison.OrdinalIgnoreCase)) return true; // 'fixed' is meaningful for Inst (handled separately) but not for others
            // treat placeholders as not meaningful
            if (t == "0" || t.Equals("n/a", StringComparison.OrdinalIgnoreCase) || t.Equals("na", StringComparison.OrdinalIgnoreCase) || t.Equals("none", StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }

        /// <summary>
        /// Creates the output file from the embedded template. If the template is a .dotx and the output uses .docx
        /// we normalize the content type from template to document to avoid Word "corruption" repair prompts.
        /// </summary>
        private static void PrepareOutputFromTemplate(string templateName, string outputPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
            var templateExt = Path.GetExtension(templateName);
            var outputExt = Path.GetExtension(outputPath);
            using var ms = TemplateResources.OpenTemplate(templateName); // returns raw bytes of template
            if (string.Equals(templateExt, ".dotx", StringComparison.OrdinalIgnoreCase) && string.Equals(outputExt, ".docx", StringComparison.OrdinalIgnoreCase))
            {
                using var working = new MemoryStream();
                ms.CopyTo(working); working.Position = 0;
                using (var zip = new ZipArchive(working, ZipArchiveMode.Update, true))
                {
                    var ctEntry = zip.GetEntry("[Content_Types].xml");
                    if (ctEntry != null)
                    {
                        string xml;
                        using (var entryStream = ctEntry.Open())
                        using (var sr = new StreamReader(entryStream)) xml = sr.ReadToEnd();
                        if (xml.Contains("template.main+xml", StringComparison.OrdinalIgnoreCase))
                        {
                            xml = xml.Replace("application/vnd.openxmlformats-officedocument.wordprocessingml.template.main+xml", "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml", StringComparison.OrdinalIgnoreCase);
                            ctEntry.Delete();
                            var newEntry = zip.CreateEntry("[Content_Types].xml");
                            using var sw = new StreamWriter(newEntry.Open());
                            sw.Write(xml);
                        }
                    }
                }
                File.WriteAllBytes(outputPath, working.ToArray());
            }
            else
            {
                using var fs = File.Create(outputPath);
                ms.Position = 0; ms.CopyTo(fs);
            }
        }

        private static int ReplaceTagsForBreaker(Project? project, LVCB breaker, IEnumerable<OpenXmlElement> scopeElements, BreakerLabelGenerationResult result, bool clearUnmatched)
        {
            int replaced = 0;
            var map = BuildValueMap(project, breaker);
            foreach (var sdt in scopeElements.SelectMany(e => e.Descendants<SdtElement>()))
            {
                var tag = GetTag(sdt);
                if (string.IsNullOrWhiteSpace(tag)) continue;
                if (map.TryGetValue(tag, out var value))
                {
                    foreach (var t in sdt.Descendants<Text>()) t.Text = value ?? string.Empty;
                    replaced++;
                    if (string.IsNullOrEmpty(value)) result.MissingMappedValues.Add(tag);
                }
                else if (IsKnownPrefix(tag))
                {
                    // Recognized prefix but property not found
                    if (clearUnmatched) foreach (var t in sdt.Descendants<Text>()) t.Text = string.Empty;
                    result.UnknownTags.Add(tag);
                }
            }
            return replaced;
        }

        private static Dictionary<string, string?> BuildValueMap(Project? project, LVCB b)
        {
            var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            if (b != null)
            {
                // Add all LVCB properties (including flattened TripUnitXxx properties)
                foreach (var kvp in LvcbProps)
                {
                    var raw = kvp.Value.GetValue(b);
                    var propName = kvp.Value.Name;
                    
                    // Add as LVCB.PropertyName
                    var lvcbTag = $"LVCB.{propName}";
                    var formatted = Format(raw);
                    formatted = AppendUnitIfNeeded(lvcbTag, formatted);
                    dict[lvcbTag] = formatted;
                    
                    // Also add TripUnitXxx properties as TripUnit.Xxx for backward compatibility with old templates
                    if (propName.StartsWith("TripUnit", StringComparison.OrdinalIgnoreCase) && propName.Length > 8)
                    {
                        var shortName = propName.Substring(8); // Remove "TripUnit" prefix
                        var tripUnitTag = $"TripUnit.{shortName}";
                        dict[tripUnitTag] = formatted;
                    }
                }
                
                AddTripModuleDerivedTags(b, dict);

                // Effective manufacturer fallbacks (dataset often missing Manufacturer header per import warnings)
                var effectiveBreakerMfr = !string.IsNullOrWhiteSpace(b.Manufacturer) ? b.Manufacturer!.Trim() : b.TripUnitManufacturer?.Trim();
                var effectiveTripMfr = !string.IsNullOrWhiteSpace(b.TripUnitManufacturer)
                    ? b.TripUnitManufacturer!.Trim()
                    : effectiveBreakerMfr; // fallback to breaker mfr

                // Expose effective manufacturers explicitly
                dict["LVCB.ManufacturerEffective"] = effectiveBreakerMfr ?? string.Empty;
                dict["TripUnit.ManufacturerEffective"] = effectiveTripMfr ?? string.Empty;

                // Composite display tags using effective manufacturers
                var breakerTypeDisplay = ComposeBreakerTypeDisplay(effectiveBreakerMfr, b.Style);
                dict["LVCB.BreakerTypeDisplay"] = breakerTypeDisplay;
                // Legacy template compatibility: also populate BreakerType if present
                if (dict.ContainsKey("LVCB.BreakerType")) dict["LVCB.BreakerType"] = breakerTypeDisplay;

                // Trip unit type display
                var hasTripUnitData = !string.IsNullOrWhiteSpace(b.TripUnitType) || !string.IsNullOrWhiteSpace(b.TripUnitStyle);
                if (hasTripUnitData)
                {
                    var tripTypeDisplay = ComposeBreakerTypeDisplay(effectiveTripMfr, b.TripUnitStyle ?? b.TripUnitType);
                    dict["TripUnit.TripTypeDisplay"] = tripTypeDisplay;
                    if (dict.ContainsKey("TripUnit.Type")) dict["TripUnit.Type"] = tripTypeDisplay; // override legacy Type tag
                    if (dict.ContainsKey("LVCB.Style")) dict["LVCB.Style"] = breakerTypeDisplay; // show Mfr + Style
                    if (dict.ContainsKey("TripUnit.Style")) dict["TripUnit.Style"] = tripTypeDisplay; // show TripUnit Mfr + Style
                }
                else
                {
                    dict["TripUnit.TripTypeDisplay"] = string.Empty;
                    if (dict.ContainsKey("LVCB.Style")) dict["LVCB.Style"] = breakerTypeDisplay;
                }
            }
            if (project != null)
            {
                foreach (var kvp in ProjectProps)
                {
                    var pt = kvp.Value.PropertyType;
                    if (pt == typeof(DataSet)) continue;
                    var raw = kvp.Value.GetValue(project);
                    var tag = $"Project.{kvp.Value.Name}";
                    var formatted = Format(raw); // typically no units for project metadata
                    dict[tag] = formatted;
                }
                if (project.Properties != null)
                {
                    foreach (var pkvp in project.Properties)
                    {
                        var tag = $"Project.Prop_{pkvp.Key}";
                        dict[tag] = pkvp.Value; // treat as pre-formatted text
                    }
                }
            }
            return dict;
        }

        // Implements legacy VBA logic for trip module descriptor (tmpStr / tmpStr2)
        private static void AddTripModuleDerivedTags(LVCB breaker, Dictionary<string, string?> dict)
        {
            var hasTripUnitData = !string.IsNullOrWhiteSpace(breaker.TripUnitType) || !string.IsNullOrWhiteSpace(breaker.TripUnitStyle);
            if (!hasTripUnitData)
            {
                dict["LVCB.MfrStyleLabel"] = (breaker.Manufacturer + " " + breaker.Style).Trim();
                dict["TripUnit.TripModuleInfo"] = string.Empty;
                return;
            }
            
            // Base label: Manufacturer + Style
            string baseLabel = string.Join(" ", new[]{breaker.Manufacturer, breaker.Style}.Where(s=>!string.IsNullOrWhiteSpace(s))).Trim();
            string? tripType = breaker.TripUnitType?.Trim();
            string? tripStyle = breaker.TripUnitStyle?.Trim();
            bool hasTripType = !string.IsNullOrWhiteSpace(tripType);
            bool hasTripStyle = !string.IsNullOrWhiteSpace(tripStyle);
            string tripTypeLower = (tripType ?? string.Empty).Trim().ToLowerInvariant();
            bool tripTypeIsStd = tripTypeLower == "(std)"; // replicates VBA LCase compare
            
            // Determine if style already contains trip style (case-insensitive contains)
            bool styleContainsTripStyle = false;
            if (hasTripStyle && !string.IsNullOrWhiteSpace(breaker.Style))
            {
                styleContainsTripStyle = breaker.Style!.IndexOf(tripStyle!, StringComparison.OrdinalIgnoreCase) >= 0;
            }
            
            // Show trip module only if breaker considered adjustable by heuristic
            bool isAdjustable = IsEffectivelyAdjustable(breaker, out _);
            string moduleInfo = string.Empty;
            
            if (isAdjustable)
            {
                // If tripType not (std) OR style does not already contain tripStyle add "/" + newline separator per legacy logic
                if ((!tripTypeIsStd) || !styleContainsTripStyle)
                {
                    baseLabel += "/" + Environment.NewLine;
                }
                // Append tripType if not (std)
                if (hasTripType && !tripTypeIsStd)
                {
                    baseLabel += tripType;
                }
                // Append tripStyle if not already present inside breaker style
                string tmpStr2 = string.Empty; // intermediate per legacy
                if (hasTripStyle && !styleContainsTripStyle)
                {
                    if (!tripTypeIsStd && hasTripType) baseLabel += " ";
                    baseLabel += tripStyle;
                    tmpStr2 = tripTypeIsStd ? string.Empty : tripType ?? string.Empty;
                }
                // Build moduleInfo (final tmpStr2)
                if (hasTripStyle)
                {
                    var pref = string.IsNullOrWhiteSpace(breaker.Manufacturer) ? string.Empty : breaker.Manufacturer!.Trim() + " ";
                    if (!string.IsNullOrWhiteSpace(tmpStr2))
                        moduleInfo = $"{pref}{tripStyle} ({tmpStr2})";
                    else
                        moduleInfo = $"{pref}{tripStyle} Trip Module";
                }
            }
            dict["LVCB.MfrStyleLabel"] = baseLabel;
            dict["TripUnit.TripModuleInfo"] = moduleInfo;
        }

        private static string? AppendUnitIfNeeded(string tag, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            if (!UnitSuffixes.TryGetValue(tag, out var unit) || string.IsNullOrWhiteSpace(unit)) return value;
            // Avoid double appending if already ends with unit (case-insensitive) or non-digit final char
            var trimmed = value.Trim();
            if (!char.IsDigit(trimmed[^1])) return value; // only append when last char is digit
            if (trimmed.EndsWith(unit, StringComparison.OrdinalIgnoreCase)) return value; // already present
            return trimmed + unit; // no space (e.g. 400A)
        }

        private static string? Format(object? val)
        {
            if (val == null) return string.Empty;
            if (val is bool b) return b ? "True" : "False";
            if (val is DateTime dt) return dt.ToString("yyyy-MM-dd");
            return val.ToString();
        }

        private static string? GetTag(SdtElement sdt)
        {
            return sdt.SdtProperties?.GetFirstChild<Tag>()?.Val?.Value
                ?? sdt.SdtProperties?.GetFirstChild<SdtAlias>()?.Val?.Value;
        }

        private static bool IsKnownTag(string? tag) => !string.IsNullOrWhiteSpace(tag) && IsKnownPrefix(tag!);
        private static bool IsKnownPrefix(string tag) => tag.StartsWith("LVCB.", StringComparison.OrdinalIgnoreCase) || tag.StartsWith("TripUnit.", StringComparison.OrdinalIgnoreCase) || tag.StartsWith("Project.", StringComparison.OrdinalIgnoreCase);

        private static string ComposeBreakerTypeDisplay(string? manufacturer, string? style)
        {
            var m = (manufacturer ?? string.Empty).Trim();
            var s = (style ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(m)) return s; // no manufacturer
            if (string.IsNullOrEmpty(s)) return m; // no style
            // Avoid duplicating manufacturer if already at start of style or contained (case-insensitive)
            if (s.StartsWith(m + " ", StringComparison.OrdinalIgnoreCase) || string.Equals(s, m, StringComparison.OrdinalIgnoreCase))
                return s; // already prefixed
            return m + " " + s;
        }
    }
}
