using System;

namespace Augury.Lucene
{
    /// <summary>
    /// Highly optimized bounded implementation of the Jaro-Winkler distance.
    /// Returns estimates for values below the boundary.
    /// </summary>
    /// <see cref="http://ceur-ws.org/Vol-1317/om2014_Tpaper4.pdf">
    /// Time-Efficient Execution of Bounded Jaro-Winkler Distances
    /// Kevin Dreßler and Axel-Cyrille Ngonga Ngomo
    /// University of Leipzig
    /// </see>
    public class BoundedJaroWinkler : JaroWinkler
    {
        protected const int MaxWordSize = 35;
        protected const double Threshold = 0.75;
        protected static readonly bool[][][] Filter;

        //Removing this can save ~50 Kb, but is a tiny bit slower.
        static BoundedJaroWinkler()
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
        /// <param name="x">The first word.</param>
        /// <param name="y">The second word.</param>
        /// <returns>The bounded similarity score.</returns>
        public override double Similarity(string x, string y)
        {
            string min, max;
            OrderStrings(x, y, out min, out max);
            var maxLen = max.Length;
            var minLen = min.Length;

            if (maxLen > 30) { return 0.0; }

            var prefix = SharedPrefixLength(min, max);

            var filter = Filter[maxLen][minLen][prefix];
            if (!filter) { return (prefix / (double)minLen + prefix / (double)maxLen) / 3.0; }

            int matchesInt, transpositions;
            Matches(min, max, out matchesInt, out transpositions);

            if (matchesInt == 0) { return (prefix / (double)minLen + prefix / (double)maxLen) / 3.0; }

            var matches = (double)matchesInt;
            var j = (matches / x.Length + matches / y.Length + (matches - transpositions) / matches) / 3.0;

            if (j < BoostThreshold) { return j; }

            var weighingFactor = Math.Min(0.1, 1.0 / maxLen);
            var jw = j + weighingFactor * prefix * (1 - j);
            return jw;

        }
    }
}
