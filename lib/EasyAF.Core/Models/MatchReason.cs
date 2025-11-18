namespace EasyAF.Core.Models
{
    /// <summary>
    /// Describes why a fuzzy match was made.
    /// </summary>
    /// <remarks>
    /// Used to explain match confidence to users and prioritize exact matches over fuzzy matches.
    /// </remarks>
    public enum MatchReason
    {
        /// <summary>
        /// Exact match (100% confidence).
        /// </summary>
        Exact,

        /// <summary>
        /// Case-insensitive exact match (98% confidence).
        /// </summary>
        CaseInsensitive,

        /// <summary>
        /// Normalized match - spaces, underscores, and case ignored (96% confidence).
        /// Critical for matching CSV column names like "LV Breakers" to property "LVBreakers".
        /// </summary>
        Normalized,

        /// <summary>
        /// Fuzzy match using Levenshtein distance algorithm.
        /// Confidence varies based on edit distance.
        /// </summary>
        Levenshtein,

        /// <summary>
        /// Fuzzy match using Jaro-Winkler similarity algorithm.
        /// Better for short strings and prefix matching.
        /// </summary>
        JaroWinkler,

        /// <summary>
        /// Hybrid match using multiple algorithms.
        /// Confidence is averaged from multiple scoring methods.
        /// </summary>
        Hybrid,

        /// <summary>
        /// No match found (0% confidence).
        /// </summary>
        NoMatch
    }
}
