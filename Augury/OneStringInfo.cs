using System.Collections.Generic;

namespace Augury
{
    internal class OneStringInfo
    {
        public uint OneGramCount;
        public uint N1PlusStarwStar;
        public uint N1PlusStarw;
        public NwStarCount NwStarCount;

        public IReadOnlyList<int> MostLikelies;
        public Dictionary<int, TwoStringInfo> TwoGrams;

        public OneStringInfo()
        {
            TwoGrams = new Dictionary<int, TwoStringInfo>();
            NwStarCount = new NwStarCount();
            MostLikelies = new List<int>();

            OneGramCount = 0;
            N1PlusStarwStar = 0;
            N1PlusStarw = 0;
        }
    }
}
