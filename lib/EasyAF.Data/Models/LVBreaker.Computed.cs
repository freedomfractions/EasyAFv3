using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Partial class extension for LVBreaker with computed properties.
/// </summary>
/// <remarks>
/// This file extends the auto-generated LVBreaker class with computed/derived properties
/// that are not part of the EasyPower CSV export but are useful for filtering, sorting, and reporting.
/// </remarks>
public partial class LVBreaker
{
    /// <summary>
    /// Gets whether this breaker has an adjustable trip unit.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This computed property uses the same heuristic logic as the BreakerLabelGenerator to determine
    /// if a breaker is considered adjustable based on:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Explicit indicators in the Trip field (contains "Adj", "Adjustable", or "Electronic")</description></item>
    /// <item><description>Presence of meaningful trip unit settings (LTPU Mult, LTD Band, STPU Setting, etc.)</description></item>
    /// </list>
    /// <para>
    /// <strong>Not Serialized:</strong> This property is computed at runtime and not included in JSON/XML serialization.
    /// </para>
    /// </remarks>
    /// <example>
    /// Filter for adjustable breakers in Spec module:
    /// <code>
    /// {
    ///   "FilterSpecs": [
    ///     { "PropertyPath": "IsAdjustable", "Operator": "eq", "Value": "true" }
    ///   ]
    /// }
    /// </code>
    /// </example>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [Category("Computed")]
    [Description("Indicates if breaker has adjustable trip unit")]
    public bool IsAdjustable => IsEffectivelyAdjustable(this);

    /// <summary>
    /// Determines if a breaker has an adjustable trip unit based on heuristics.
    /// </summary>
    /// <remarks>
    /// This is the same logic used by BreakerLabelGenerator.IsEffectivelyAdjustable().
    /// Checks Trip field and presence of meaningful trip unit settings.
    /// </remarks>
    private static bool IsEffectivelyAdjustable(LVBreaker breaker)
    {
        if (breaker == null) return false;

        // Check explicit adjustable flag from Trip field
        bool flagged = false;
        if (!string.IsNullOrWhiteSpace(breaker.Trip))
        {
            var adj = breaker.Trip.Trim().ToLowerInvariant();
            flagged = adj.Contains("adj") || adj.Contains("adjustable") || adj.Contains("electronic");
        }

        // Check for meaningful trip unit settings (strict indicators)
        var strictIndicators = new[]
        {
            breaker.LTPUMult, breaker.LtdBand, breaker.LtCurve,
            breaker.STPUSetting, breaker.STPUBand, breaker.STPUI2t,
            breaker.InstSetting, breaker.TripAdjust, breaker.InstA,
            breaker.MaintSetting, breaker.GndA, breaker.GndDelay, breaker.GndI2t
        };

        bool hasIndicators = strictIndicators.Any(v => IsMeaningfulValue(v));

        // Explicit flag OR indicators present
        if (!(flagged || hasIndicators)) return false;

        // Special case: If InstSetting is "fixed" and only indicator is InstA -> NOT adjustable
        if (string.Equals(breaker.InstSetting?.Trim(), "fixed", StringComparison.OrdinalIgnoreCase) && 
            strictIndicators.Count(v => IsMeaningfulValue(v)) == 1 && 
            IsMeaningfulValue(breaker.InstA))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if a string value is considered meaningful (not empty, not placeholder).
    /// </summary>
    private static bool IsMeaningfulValue(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        var t = s.Trim();
        if (t.Length == 0) return false;
        if (t == "0" || t.Equals("n/a", StringComparison.OrdinalIgnoreCase) || 
            t.Equals("na", StringComparison.OrdinalIgnoreCase) || 
            t.Equals("none", StringComparison.OrdinalIgnoreCase))
            return false;
        return true;
    }
}
