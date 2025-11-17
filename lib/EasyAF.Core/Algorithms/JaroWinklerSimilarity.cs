using System;

namespace EasyAF.Core.Algorithms
{
    /// <summary>
    /// Calculates Jaro-Winkler similarity between two strings.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Jaro-Winkler similarity is a string metric measuring similarity between two strings.
    /// It gives higher scores to strings that match from the beginning (prefix matching).
    /// </para>
    /// <para>
    /// <strong>Best For:</strong>
    /// - Short strings (names, identifiers)
    /// - Cases where prefix matching is important (e.g., "Id" vs "ID", "Name" vs "NAME")
    /// - Typos at the end of strings
    /// </para>
    /// <para>
    /// <strong>Time Complexity:</strong> O(m * n) where m and n are the lengths of the two strings.
    /// </para>
    /// </remarks>
    public static class JaroWinklerSimilarity
    {
        /// <summary>
        /// Calculates the Jaro-Winkler similarity between two strings.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="target">The target string.</param>
        /// <param name="caseSensitive">Whether to perform case-sensitive comparison (default: false).</param>
        /// <param name="prefixScale">Scaling factor for prefix matching (default: 0.1, max: 0.25).</param>
        /// <returns>
        /// Similarity score between 0.0 and 1.0:
        /// - 1.0 = identical strings
        /// - 0.0 = completely different
        /// </returns>
        /// <example>
        /// <code>
        /// double score = JaroWinklerSimilarity.Calculate("Id", "ID");
        /// // Returns: 1.0 (case-insensitive exact match)
        /// 
        /// score = JaroWinklerSimilarity.Calculate("Name", "NAME_FULL");
        /// // Returns: ~0.87 (high prefix match)
        /// </code>
        /// </example>
        public static double Calculate(string source, string target, bool caseSensitive = false, double prefixScale = 0.1)
        {
            if (string.IsNullOrEmpty(source) && string.IsNullOrEmpty(target))
                return 1.0; // Both empty = perfect match

            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
                return 0.0; // One empty = no match

            // Make case-insensitive if requested
            if (!caseSensitive)
            {
                source = source.ToLowerInvariant();
                target = target.ToLowerInvariant();
            }

            // Exact match shortcut
            if (source == target)
                return 1.0;

            int sourceLength = source.Length;
            int targetLength = target.Length;

            // Calculate match window (half the length of the longer string, minus 1)
            int matchWindow = Math.Max(sourceLength, targetLength) / 2 - 1;
            if (matchWindow < 1)
                matchWindow = 1;

            bool[] sourceMatches = new bool[sourceLength];
            bool[] targetMatches = new bool[targetLength];

            int matches = 0;
            int transpositions = 0;

            // Find matches
            for (int i = 0; i < sourceLength; i++)
            {
                int start = Math.Max(0, i - matchWindow);
                int end = Math.Min(i + matchWindow + 1, targetLength);

                for (int j = start; j < end; j++)
                {
                    if (targetMatches[j] || source[i] != target[j])
                        continue;

                    sourceMatches[i] = true;
                    targetMatches[j] = true;
                    matches++;
                    break;
                }
            }

            if (matches == 0)
                return 0.0;

            // Count transpositions
            int k = 0;
            for (int i = 0; i < sourceLength; i++)
            {
                if (!sourceMatches[i])
                    continue;

                while (!targetMatches[k])
                    k++;

                if (source[i] != target[k])
                    transpositions++;

                k++;
            }

            // Calculate Jaro similarity
            double jaro = ((double)matches / sourceLength +
                          (double)matches / targetLength +
                          (double)(matches - transpositions / 2) / matches) / 3.0;

            // Calculate common prefix length (up to 4 characters)
            int prefixLength = 0;
            int maxPrefix = Math.Min(4, Math.Min(sourceLength, targetLength));

            for (int i = 0; i < maxPrefix; i++)
            {
                if (source[i] == target[i])
                    prefixLength++;
                else
                    break;
            }

            // Calculate Jaro-Winkler similarity
            // Boost score if strings share a common prefix
            double jaroWinkler = jaro + (prefixLength * prefixScale * (1.0 - jaro));

            return jaroWinkler;
        }
    }
}
