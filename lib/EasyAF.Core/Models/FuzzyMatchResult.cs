namespace EasyAF.Core.Models
{
    /// <summary>
    /// Result of a fuzzy string matching operation.
    /// </summary>
    /// <remarks>
    /// Contains the matched strings, confidence score, and explanation of why the match was made.
    /// Used for intelligent auto-mapping, search suggestions, and user feedback.
    /// </remarks>
    public class FuzzyMatchResult
    {
        /// <summary>
        /// Gets or sets the source string (query).
        /// </summary>
        /// <example>"BusName"</example>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the target string (candidate that was matched).
        /// </summary>
        /// <example>"BUS_NAME"</example>
        public string Target { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the confidence score (0.0 to 1.0).
        /// </summary>
        /// <remarks>
        /// <para>
        /// Score interpretation:
        /// - 1.0: Perfect match (exact or case-insensitive exact)
        /// - 0.8-0.99: High confidence (very similar)
        /// - 0.6-0.79: Medium confidence (somewhat similar)
        /// - 0.4-0.59: Low confidence (weak match)
        /// - 0.0-0.39: Very low confidence (probably not a match)
        /// </para>
        /// <para>
        /// Recommended threshold for auto-mapping: 0.6 or higher.
        /// </para>
        /// </remarks>
        public double Score { get; set; }

        /// <summary>
        /// Gets or sets the reason/algorithm used for matching.
        /// </summary>
        /// <remarks>
        /// Helps users understand why a match was suggested.
        /// </remarks>
        public MatchReason Reason { get; set; }

        /// <summary>
        /// Gets a formatted display string for the match.
        /// </summary>
        /// <example>"BusName ? BUS_NAME (95% confidence, Case-Insensitive)"</example>
        public string DisplayText => $"{Source} ? {Target} ({Score:P0} confidence, {Reason})";

        /// <summary>
        /// Returns a string representation of this match result.
        /// </summary>
        public override string ToString() => DisplayText;
    }
}
