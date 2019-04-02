using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Search
{
    public class Kmp
    {
        /// <summary>
        /// Searches a pattern for a sequence using the KMP algorithm.
        /// Returns the index of the first match, or -1 if no match is found.
        /// Runs in time complexity O(m+n) where m = text.Length and n = pattern.Length
        /// </summary>
        public static int Search<T>(T[] text, T[] pattern)
        {
            var comparer = EqualityComparer<T>.Default;
            var lps = ComputeLps(pattern, comparer);
            int i = 0;
            int j = 0;
            while(i < text.Length && j < pattern.Length)
            {
                if (comparer.Equals(text[i], pattern[j]))
                {
                    ++i;
                    ++j;
                }
                else if (j != 0)
                {
                    j = lps[j - 1];
                }
                else
                {
                    ++i;
                }
                if (j == pattern.Length)
                    return i;
            }
            return -1;
        }
        /// <summary>
        /// Searches a pattern for a sequence using the KMP algorithm.
        /// Returns the indices of all matches in an array.
        /// Runs in time complexity O(m+n) where m = text.Length and n = pattern.Length
        /// </summary>
        public static List<int> SearchAll<T>(T[] text, T[] pattern)
        {
            var comparer = EqualityComparer<T>.Default;
            var lps = ComputeLps(pattern, comparer);
            var matches = new List<int>();
            int i = 0;
            int j = 0;
            while (i < text.Length && j < pattern.Length)
            {
                if (comparer.Equals(text[i], pattern[j]))
                {
                    ++i;
                    ++j;
                }
                else if (j != 0)
                {
                    j = lps[j - 1];
                }
                else
                {
                    ++i;
                }
                if (j == pattern.Length)
                    matches.Add(i);
            }
            return matches;
        }

        /// <summary> Computes the lps array for the given pattern </summary>
        private static int[] ComputeLps<T>(T[] pattern, EqualityComparer<T> comparer)
        {
            var lps = new int[pattern.Length];
            int j = 0;
            for (int i = 1; i < pattern.Length;)
            {
                if (comparer.Equals(pattern[i], pattern[j]))
                    lps[i++] = ++j;
                else if (j != 0)
                    j = lps[j - 1];
                else
                    lps[i++] = 0;
            }
            return lps;
        }
    }
}
