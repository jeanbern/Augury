using System.Collections.Generic;

namespace Augury
{
    internal class TwoStringInfo
    {
        public uint TwoGramCount;
        public uint N1PlusStarww;
        public NwStarCount NwwStarCount;
        public Dictionary<int, uint> ThreeGramCounts;

        public IReadOnlyList<int> MostLikelies;

        public TwoStringInfo()
        {
            TwoGramCount = 0;
            N1PlusStarww = 0;
            NwwStarCount = new NwStarCount();
            ThreeGramCounts = new Dictionary<int, uint>();
            MostLikelies = new List<int>();
        }
    }
}
