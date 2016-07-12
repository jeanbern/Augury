using Augury.Base;
using System;
using System.Linq;

namespace Augury.Lucene
{
    public class JaroWinkler : IStringMetric
    {
        protected const double BoostThreshold = 0.7;

        protected static void OrderStrings(string x, string y, out string min, out string max)
        {
            var maxLen = x.Length;
            var minLen = y.Length;
            if (maxLen > minLen)
            {
                max = x;
                min = y;
            }
            else
            {
                max = y;
                min = x;
                minLen = maxLen;
                maxLen = max.Length;
            }
        }

        protected static int SharedPrefixLength(string min, string max)
        {
            var minLen = min.Length;
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

            return prefix;
        }

        /// <summary>
        /// Returns the similarity between two words.
        /// </summary>
        /// <param name="x">The first word.</param>
        /// <param name="y">The second word.</param>
        /// <returns>The similarity score.</returns>
        public virtual double Similarity(string s1, string s2)
        {
            string min, max;
            OrderStrings(s1, s2, out min, out max);
            var maxLen = max.Length;
            var minLen = min.Length;
            
            var prefix = SharedPrefixLength(min, max);
            
            int matchesInt, transpositions;
            Matches(min, max, out matchesInt, out transpositions);
            
            var matches = (double)matchesInt;
            var j = (matches / s1.Length + matches / s2.Length + (matches - transpositions) / matches) / 3.0;

            if (j < BoostThreshold) { return j; }

            var weighingFactor = Math.Min(0.1, 1.0 / maxLen);
            var jw = j + weighingFactor * prefix * (1 - j);
            return jw;
        }

        protected static void Matches(string max, string min, out int matches, out int transpositions)
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
