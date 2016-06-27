using System;
using System.Linq;

namespace Augury.Lucene
{
    /// <summary>
    /// Highly optimized implementation of the Jaro-Winkler distance.
    /// Includes a bounded version which returns estimates for values below the boundary.
    /// </summary>
    /// <see cref="http://ceur-ws.org/Vol-1317/om2014_Tpaper4.pdf">
    /// Time-Efficient Execution of Bounded Jaro-Winkler Distances
    /// Kevin Dreßler and Axel-Cyrille Ngonga Ngomo
    /// University of Leipzig
    /// </see>
    public static class JaroWinkler
    {
        private const int MaxWordSize = 35;
        private const double BoostThreshold = 0.7;
        private const double Threshold = 0.75;
        private static readonly bool[][][] Filter;

        //Removing this can save ~50 Kb, but is a tiny bit slower.
        static JaroWinkler()
        {
            Filter = new bool[MaxWordSize][][];
            for (var x = 0; x < MaxWordSize; x++)
            {
                Filter[x] = new bool[x + 1][];
                for (var y = 0; y <= x; y++)
                {
                    Filter[x][y] = new bool[y + 1];

                    var weighingFactor = Math.Min(0.1, 1.0 / x);
                    var dj = 2.0 / 3.0 + y / (3.0 * x);
                    for (var prefix = 0; prefix <= y; prefix++)
                    {
                        var lp = prefix * weighingFactor;
                        Filter[x][y][prefix] = dj + lp * (1.0 - dj) > Threshold;
                    }
                }
            }
        }

        public static int MaxLengthForPrefix(int match, int s1, int transpositions)
        {
            var denom = 3*BoostThreshold - (match - transpositions)/((double) match) - (match)/((double) (s1));
            return (int) Math.Floor(match/denom);
        }

        /// <summary>
        /// Returns the similarity between two words. 
        /// An estimate is used if the value is guaranteed to be below a threshold.
        /// </summary>
        /// <param name="s1">The first word.</param>
        /// <param name="s2">The second word.</param>
        /// <returns>The bounded similarity score.</returns>
        public static double BoundedSimilarity(string s1, string s2)
        {
            string max, min;
            var maxLen = s1.Length;
            var minLen = s2.Length;
            if (maxLen > minLen)
            {
                max = s1;
                min = s2;
            }
            else
            {
                max = s2;
                min = s1;
                minLen = maxLen;
                maxLen = max.Length;
            }

            if (maxLen > 30) { return 0.0; }

            var prefix = 0;
            for (var mi = 0; mi < minLen; mi++)
            {
                if (min[mi] == max[mi])
                {
                    prefix++;
                }
                else
                {
                    break;
                }
            }

            var filter = Filter[maxLen][minLen][prefix];
            if (!filter) { return (prefix / (double)minLen + prefix / (double)maxLen) / 3.0; }

            int matchesInt, transpositions;
            Matches(min, max, out matchesInt, out transpositions);

            if (matchesInt == 0) { return (prefix / (double)minLen + prefix / (double)maxLen) / 3.0; }

            var matches = (double)matchesInt;
            var j = (matches / s1.Length + matches / s2.Length + (matches - transpositions) / matches) / 3.0;

            if (j < BoostThreshold) { return j; }

            var weighingFactor = Math.Min(0.1, 1.0 / maxLen);
            var jw = j + weighingFactor * prefix * (1 - j);
            return jw;
        }

        private static void Matches(string max, string min, out int matches, out int transpositions)
        {
            var minLen = min.Length;
            var maxLen = max.Length;
            var range = Math.Max(maxLen / 2 - 1, 0);
            var matchIndexes = new int[minLen];

            for (var i = 0; i < matchIndexes.Length; i++)
            {
                matchIndexes[i] = -1;
            }

            var matchFlags = new bool[maxLen];
            matches = 0;
            transpositions = 0;

            for (var mi = 0; mi < minLen; mi++)
            {
                var c1 = min[mi];
                for (int xi = Math.Max(mi - range, 0), xn = Math.Min(mi + range + 1, maxLen); xi < xn; xi++)
                {
                    if (matchFlags[xi] || c1 != max[xi])
                    {
                        continue;
                    }

                    matchIndexes[mi] = xi;
                    matchFlags[xi] = true;
                    matches++;
                    break;
                }
            }


            if (matches == 0)
            {
                return;
            }

            var copy = matchIndexes.Where(matchIndex => matchIndex != -1).OrderBy(x => x).ToArray();

            var index = 0;
            foreach (var matchIndex in matchIndexes)
            {
                if (matchIndex == copy[index])
                {
                    index++;
                    if (index >= matches)
                    {
                        return;
                    }

                    continue;
                }

                if (matchIndex != -1)
                {
                    transpositions++;
                }
            }
        }
    }
}
