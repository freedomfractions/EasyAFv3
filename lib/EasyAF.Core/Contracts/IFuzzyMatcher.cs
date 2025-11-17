using System.Collections.Generic;
using EasyAF.Core.Models;

namespace EasyAF.Core.Contracts
{
    /// <summary>
    /// Service for fuzzy string matching and similarity scoring.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Provides intelligent string matching using multiple algorithms (Levenshtein, Jaro-Winkler)
    /// to find similar strings even when they don't match exactly. Useful for:
    /// - Auto-mapping columns to properties
    /// - Search suggestions and autocomplete
    /// - Typo tolerance in user input
    /// - Finding related items by name
    /// </para>
    /// <para>
    /// The service uses a hybrid approach combining multiple algorithms to provide
    /// robust matching across different use cases.
    /// </para>
    /// </remarks>
    public interface IFuzzyMatcher
    {
        /// <summary>
        /// Matches a single source string against a target string.
        /// </summary>
        /// <param name="source">The source string (query).</param>
        /// <param name="target">The target string (candidate).</param>
        /// <param name="caseSensitive">Whether to perform case-sensitive comparison (default: false).</param>
        /// <returns>
        /// A FuzzyMatchResult containing the match score (0.0-1.0) and reason.
        /// </returns>
        /// <example>
        /// <code>
        /// var result = fuzzyMatcher.Match("BusName", "BUS_NAME");
        /// if (result.Score >= 0.6)
        /// {
        ///     // High confidence match
        ///     Console.WriteLine($"Match found: {result.DisplayText}");
        /// }
        /// </code>
        /// </example>
        FuzzyMatchResult Match(string source, string target, bool caseSensitive = false);

        /// <summary>
        /// Finds the best matching candidates from a collection of target strings.
        /// </summary>
        /// <param name="query">The search query.</param>
        /// <param name="candidates">Collection of candidate strings to search.</param>
        /// <param name="maxResults">Maximum number of results to return (default: 5).</param>
        /// <param name="minScore">Minimum similarity score threshold (default: 0.0, range: 0.0-1.0).</param>
        /// <param name="caseSensitive">Whether to perform case-sensitive comparison (default: false).</param>
        /// <returns>
        /// List of FuzzyMatchResult ordered by score (highest first), limited to maxResults.
        /// Returns empty list if no matches meet the minScore threshold.
        /// </returns>
        /// <example>
        /// <code>
        /// var columns = new[] { "BUS_NAME", "BUS_ID", "VOLTAGE_KV", "DESCRIPTION" };
        /// var matches = fuzzyMatcher.FindBestMatches("Name", columns, maxResults: 3, minScore: 0.6);
        /// 
        /// foreach (var match in matches)
        /// {
        ///     Console.WriteLine(match.DisplayText);
        ///     // Output: Name ? BUS_NAME (85% confidence, Hybrid)
        /// }
        /// </code>
        /// </example>
        List<FuzzyMatchResult> FindBestMatches(
            string query,
            IEnumerable<string> candidates,
            int maxResults = 5,
            double minScore = 0.0,
            bool caseSensitive = false);
    }
}
