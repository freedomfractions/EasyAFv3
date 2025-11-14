using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace EasyAF.Shell.Helpers;

/// <summary>
/// Provides comprehensive search functionality with exact, wildcard, and fuzzy matching.
/// </summary>
public static class SearchHelper
{
    /// <summary>
    /// Performs a comprehensive search match against a target string.
    /// Supports exact substring, wildcard (* and ?), and fuzzy matching.
    /// </summary>
    /// <param name="searchTerm">The search term entered by user.</param>
    /// <param name="target">The target string to search within.</param>
    /// <param name="fuzzyThreshold">Fuzzy match threshold (0.0 to 1.0). Default is 0.7.</param>
    /// <returns>True if the target matches the search term.</returns>
    public static bool IsMatch(string searchTerm, string target, double fuzzyThreshold = 0.7)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return true; // Empty search matches everything

        if (string.IsNullOrWhiteSpace(target))
            return false;

        // Normalize to lowercase for case-insensitive search
        var search = searchTerm.Trim().ToLowerInvariant();
        var text = target.ToLowerInvariant();

        // 1. EXACT MATCH: Simple substring search (highest priority)
        if (text.Contains(search))
            return true;

        // 2. WILDCARD MATCH: Check if search contains wildcards
        if (search.Contains('*') || search.Contains('?'))
        {
            return WildcardMatch(search, text);
        }

        // 3. FUZZY MATCH: Character sequence with gaps allowed
        return FuzzyMatch(search, text, fuzzyThreshold);
    }

    /// <summary>
    /// Performs wildcard matching with * (zero or more chars) and ? (single char).
    /// </summary>
    private static bool WildcardMatch(string pattern, string text)
    {
        try
        {
            // Convert wildcard pattern to regex
            // Escape all regex special chars except * and ?
            var regexPattern = "^" + Regex.Escape(pattern)
                .Replace("\\*", ".*")  // * becomes .* (zero or more of any char)
                .Replace("\\?", ".")   // ? becomes . (exactly one char)
                + "$";

            return Regex.IsMatch(text, regexPattern, RegexOptions.IgnoreCase);
        }
        catch
        {
            // If regex fails, fall back to exact match
            return text.Contains(pattern.Replace("*", "").Replace("?", ""));
        }
    }

    /// <summary>
    /// Performs fuzzy matching using character sequence matching.
    /// Returns true if search characters appear in order within target (with gaps allowed).
    /// Uses a scoring system to determine match quality.
    /// </summary>
    private static bool FuzzyMatch(string search, string target, double threshold)
    {
        if (search.Length > target.Length)
            return false;

        var score = CalculateFuzzyScore(search, target);
        return score >= threshold;
    }

    /// <summary>
    /// Calculates a fuzzy match score (0.0 to 1.0) based on:
    /// - Character sequence presence
    /// - Proximity of matching characters
    /// - Word boundary bonuses
    /// </summary>
    private static double CalculateFuzzyScore(string search, string target)
    {
        int searchIndex = 0;
        int lastMatchIndex = -1;
        double score = 0;
        int consecutiveMatches = 0;

        for (int i = 0; i < target.Length && searchIndex < search.Length; i++)
        {
            if (target[i] == search[searchIndex])
            {
                // Character match found
                double charScore = 1.0;

                // BONUS: Word boundary (character after space, underscore, or start)
                if (i == 0 || IsWordBoundary(target[i - 1]))
                {
                    charScore += 0.5;
                }

                // BONUS: Consecutive characters
                if (lastMatchIndex == i - 1)
                {
                    consecutiveMatches++;
                    charScore += (0.2 * consecutiveMatches);
                }
                else
                {
                    consecutiveMatches = 0;
                }

                // BONUS: Close proximity to last match
                if (lastMatchIndex >= 0)
                {
                    int distance = i - lastMatchIndex;
                    if (distance <= 3)
                    {
                        charScore += 0.3 / distance;
                    }
                }

                score += charScore;
                lastMatchIndex = i;
                searchIndex++;
            }
        }

        // If we didn't match all search characters, it's not a match
        if (searchIndex < search.Length)
            return 0;

        // Normalize score: divide by maximum possible score
        double maxScore = search.Length * 2.0; // Base + bonuses
        return Math.Min(score / maxScore, 1.0);
    }

    /// <summary>
    /// Checks if a character is a word boundary.
    /// </summary>
    private static bool IsWordBoundary(char c)
    {
        return char.IsWhiteSpace(c) || 
               c == '_' || 
               c == '-' || 
               c == '.' || 
               c == '\\' || 
               c == '/';
    }

    /// <summary>
    /// Multi-field search: searches across multiple text fields.
    /// </summary>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="fields">The fields to search within.</param>
    /// <param name="fuzzyThreshold">Fuzzy match threshold.</param>
    /// <returns>True if any field matches.</returns>
    public static bool IsMatchAny(string searchTerm, double fuzzyThreshold = 0.7, params string[] fields)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return true;

        return fields.Any(field => IsMatch(searchTerm, field ?? string.Empty, fuzzyThreshold));
    }
}
