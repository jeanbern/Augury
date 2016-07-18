using Augury.Comparers;
using System.Collections.Generic;
using System.Linq;

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

        public override bool Equals(object obj)
        {
            var other = obj as TwoStringInfo;
            return other != null && Equals(other);
        }

        protected bool Equals(TwoStringInfo other)
        {
            return TwoGramCount == other.TwoGramCount && N1PlusStarww == other.N1PlusStarww && 
                NwwStarCount.Equals(other.NwwStarCount) &&
                ThreeGramCounts.SequenceEqual(other.ThreeGramCounts, new KeyValuePairIntUintComparer()) && 
                MostLikelies.SequenceEqual(other.MostLikelies);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) TwoGramCount;
                hashCode = (hashCode*397) ^ (int) N1PlusStarww;
                hashCode = (hashCode*397) ^ NwwStarCount.GetHashCode();
                hashCode = ThreeGramCounts?.Aggregate(hashCode, (current, threeGram) => (int)((((current * 397) ^ threeGram.Key) * 397) ^ threeGram.Value)) ?? hashCode;
                hashCode = MostLikelies?.Aggregate(hashCode, (current, count) => (current * 397) ^ count) ?? hashCode;
                return hashCode;
            }
        }
    }
}
