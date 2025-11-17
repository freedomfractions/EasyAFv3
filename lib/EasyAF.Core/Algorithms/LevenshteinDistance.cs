using System;

namespace EasyAF.Core.Algorithms
{
    /// <summary>
    /// Calculates Levenshtein distance (edit distance) between two strings.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Levenshtein distance is the minimum number of single-character edits (insertions, deletions, substitutions)
    /// required to transform one string into another. This is useful for fuzzy matching when strings may have typos
    /// or slight variations.
    /// </para>
    /// <para>
    /// <strong>Time Complexity:</strong> O(m * n) where m and n are the lengths of the two strings.
    /// <strong>Space Complexity:</strong> O(min(m, n)) using optimized two-row approach.
    /// </para>
    /// <para>
    /// <strong>Use Cases:</strong>
    /// - Spell checking ("recieve" ? "receive")
    /// - Column name matching ("BusName" ? "BUS_NAME")
    /// - Typo tolerance in search
    /// </para>
    /// </remarks>
    public static class LevenshteinDistance
    {
        /// <summary>
        /// Calculates the Levenshtein distance between two strings.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="target">The target string.</param>
        /// <param name="caseSensitive">Whether to perform case-sensitive comparison (default: false).</param>
        /// <returns>The minimum number of edits required to transform source into target.</returns>
        /// <example>
        /// <code>
        /// int distance = LevenshteinDistance.Calculate("kitten", "sitting");
        /// // Returns: 3 (substitute k?s, substitute e?i, insert g)
        /// </code>
        /// </example>
        public static int Calculate(string source, string target, bool caseSensitive = false)
        {
            if (string.IsNullOrEmpty(source))
                return target?.Length ?? 0;

            if (string.IsNullOrEmpty(target))
                return source.Length;

            // Make case-insensitive if requested
            if (!caseSensitive)
            {
                source = source.ToLowerInvariant();
                target = target.ToLowerInvariant();
            }

            int sourceLength = source.Length;
            int targetLength = target.Length;

            // Optimization: Use two rows instead of full matrix
            int[] previousRow = new int[targetLength + 1];
            int[] currentRow = new int[targetLength + 1];

            // Initialize first row (distances from empty string)
            for (int j = 0; j <= targetLength; j++)
                previousRow[j] = j;

            // Calculate distances row by row
            for (int i = 1; i <= sourceLength; i++)
            {
                currentRow[0] = i; // Distance from empty string

                for (int j = 1; j <= targetLength; j++)
                {
                    int cost = (source[i - 1] == target[j - 1]) ? 0 : 1;

                    currentRow[j] = Math.Min(
                        Math.Min(
                            currentRow[j - 1] + 1,      // Insertion
                            previousRow[j] + 1),        // Deletion
                        previousRow[j - 1] + cost);     // Substitution
                }

                // Swap rows
                var temp = previousRow;
                previousRow = currentRow;
                currentRow = temp;
            }

            return previousRow[targetLength];
        }

        /// <summary>
        /// Calculates a normalized similarity score (0.0 to 1.0) based on Levenshtein distance.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="target">The target string.</param>
        /// <param name="caseSensitive">Whether to perform case-sensitive comparison (default: false).</param>
        /// <returns>
        /// Similarity score where:
        /// - 1.0 = identical strings (distance = 0)
        /// - 0.0 = completely different (distance = max length)
        /// </returns>
        /// <example>
        /// <code>
        /// double score = LevenshteinDistance.Similarity("BusName", "BUS_NAME");
        /// // Returns: ~0.625 (5 edits out of 8 chars)
        /// </code>
        /// </example>
        public static double Similarity(string source, string target, bool caseSensitive = false)
        {
            if (string.IsNullOrEmpty(source) && string.IsNullOrEmpty(target))
                return 1.0; // Both empty = perfect match

            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
                return 0.0; // One empty = no match

            int distance = Calculate(source, target, caseSensitive);
            int maxLength = Math.Max(source.Length, target.Length);

            // Normalize: (max - distance) / max
            return 1.0 - ((double)distance / maxLength);
        }
    }
}
