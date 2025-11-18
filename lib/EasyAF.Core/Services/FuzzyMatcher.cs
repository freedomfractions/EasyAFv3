using System;
using System.Collections.Generic;
using System.Linq;
using EasyAF.Core.Algorithms;
using EasyAF.Core.Contracts;
using EasyAF.Core.Models;
using Serilog;

namespace EasyAF.Core.Services
{
    /// <summary>
    /// Implementation of fuzzy string matching service using hybrid algorithm approach.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This service combines multiple string similarity algorithms to provide robust matching:
    /// - Exact match detection (100% score)
    /// - Case-insensitive exact match (95% score)
    /// - Levenshtein distance for edit-based similarity
    /// - Jaro-Winkler for prefix matching and short strings
    /// - Hybrid scoring (average of Levenshtein + Jaro-Winkler)
    /// </para>
    /// <para>
    /// The hybrid approach works well across various use cases:
    /// - Column name matching ("BusName" ? "BUS_NAME")
    /// - Typo tolerance ("recieve" ? "receive")
    /// - Abbreviation matching ("Id" ? "ID", "kV" ? "KV")
    /// - Partial matching ("Name" ? "BUS_NAME")
    /// </para>
    /// </remarks>
    public class FuzzyMatcher : IFuzzyMatcher
    {
        /// <summary>
        /// Matches a single source string against a target string.
        /// </summary>
        /// <param name="source">The source string (query).</param>
        /// <param name="target">The target string (candidate).</param>
        /// <param name="caseSensitive">Whether to perform case-sensitive comparison (default: false).</param>
        /// <returns>A FuzzyMatchResult containing the match score and reason.</returns>
        public FuzzyMatchResult Match(string source, string target, bool caseSensitive = false)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(target))
            {
                return new FuzzyMatchResult
                {
                    Source = source ?? string.Empty,
                    Target = target ?? string.Empty,
                    Score = 0.0,
                    Reason = MatchReason.NoMatch
                };
            }

            // Exact match (100% confidence)
            if (source == target)
            {
                return new FuzzyMatchResult
                {
                    Source = source,
                    Target = target,
                    Score = 1.0,
                    Reason = MatchReason.Exact
                };
            }

            // Case-insensitive exact match (98% confidence - boosted from 95% for CSV column priority)
            // This is critical for matching CSV column names like "LV Breakers" to property "LVBreakers"
            if (!caseSensitive && string.Equals(source, target, StringComparison.OrdinalIgnoreCase))
            {
                return new FuzzyMatchResult
                {
                    Source = source,
                    Target = target,
                    Score = 0.98,
                    Reason = MatchReason.CaseInsensitive
                };
            }

            // Normalized exact match (96% confidence) - NEW for CSV column matching
            // Handles "LV Breakers" ? "LVBreakers", "Fuses" ? "fuses", etc.
            var normalizedSource = NormalizeColumnName(source);
            var normalizedTarget = NormalizeColumnName(target);
            if (normalizedSource == normalizedTarget)
            {
                return new FuzzyMatchResult
                {
                    Source = source,
                    Target = target,
                    Score = 0.96,
                    Reason = MatchReason.Normalized
                };
            }

            // Calculate fuzzy match scores using both algorithms
            double levenshteinScore = LevenshteinDistance.Similarity(source, target, caseSensitive);
            double jaroWinklerScore = JaroWinklerSimilarity.Calculate(source, target, caseSensitive);

            // Hybrid approach: Average the two scores with slight bias toward Jaro-Winkler for short strings
            // Jaro-Winkler is better for prefix matching and short strings (like "Id", "kV")
            // Levenshtein is better for longer strings with edits
            double weight = (source.Length <= 4 || target.Length <= 4) ? 0.6 : 0.5; // Bias for short strings
            double hybridScore = (jaroWinklerScore * weight) + (levenshteinScore * (1 - weight));

            // Determine which algorithm contributed most to the score (for user feedback)
            MatchReason reason;
            if (Math.Abs(levenshteinScore - jaroWinklerScore) < 0.05)
            {
                reason = MatchReason.Hybrid; // Both algorithms agree
            }
            else if (jaroWinklerScore > levenshteinScore)
            {
                reason = MatchReason.JaroWinkler; // Prefix matching was better
            }
            else
            {
                reason = MatchReason.Levenshtein; // Edit distance was better
            }

            return new FuzzyMatchResult
            {
                Source = source,
                Target = target,
                Score = hybridScore,
                Reason = reason
            };
        }

        /// <summary>
        /// Normalizes a column/property name for matching by removing spaces, underscores, and converting to lowercase.
        /// This helps match CSV column names like "LV Breakers" to property names like "LVBreakers".
        /// </summary>
        /// <param name="name">The name to normalize.</param>
        /// <returns>Normalized name (lowercase, no spaces/underscores/dashes).</returns>
        private static string NormalizeColumnName(string name)
        {
            return name.Replace(" ", "")
                       .Replace("_", "")
                       .Replace("-", "")
                       .Replace("/", "")
                       .ToLowerInvariant();
        }

        /// <summary>
        /// Finds the best matching candidates from a collection of target strings.
        /// </summary>
        /// <param name="query">The search query.</param>
        /// <param name="candidates">Collection of candidate strings to search.</param>
        /// <param name="maxResults">Maximum number of results to return.</param>
        /// <param name="minScore">Minimum similarity score threshold (0.0-1.0).</param>
        /// <param name="caseSensitive">Whether to perform case-sensitive comparison.</param>
        /// <returns>List of FuzzyMatchResult ordered by score (highest first).</returns>
        public List<FuzzyMatchResult> FindBestMatches(
            string query,
            IEnumerable<string> candidates,
            int maxResults = 5,
            double minScore = 0.0,
            bool caseSensitive = false)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                Log.Debug("FindBestMatches called with empty query");
                return new List<FuzzyMatchResult>();
            }

            if (candidates == null || !candidates.Any())
            {
                Log.Debug("FindBestMatches called with no candidates");
                return new List<FuzzyMatchResult>();
            }

            try
            {
                var results = new List<FuzzyMatchResult>();

                // Match against each candidate
                foreach (var candidate in candidates)
                {
                    if (string.IsNullOrWhiteSpace(candidate))
                        continue;

                    var matchResult = Match(query, candidate, caseSensitive);

                    // Only include results that meet the minimum score threshold
                    if (matchResult.Score >= minScore)
                    {
                        results.Add(matchResult);
                    }
                }

                // Sort by score (descending) and take top N results
                var topMatches = results
                    .OrderByDescending(r => r.Score)
                    .ThenBy(r => r.Target.Length) // Prefer shorter matches for same score
                    .Take(maxResults)
                    .ToList();

                Log.Debug("FindBestMatches: query='{Query}', candidates={CandidateCount}, matches={MatchCount}, threshold={MinScore}",
                    query, candidates.Count(), topMatches.Count, minScore);

                return topMatches;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in FindBestMatches for query '{Query}'", query);
                return new List<FuzzyMatchResult>();
            }
        }
    }
}
