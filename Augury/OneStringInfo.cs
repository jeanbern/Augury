using Augury.Comparers;
using System.Collections.Generic;
using System.Linq;

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

        public override bool Equals(object obj)
        {
            var other = obj as OneStringInfo;
            return other != null && Equals(other);
        }

        protected bool Equals(OneStringInfo other)
        {
            return OneGramCount == other.OneGramCount &&
                   N1PlusStarwStar == other.N1PlusStarwStar &&
                   N1PlusStarw == other.N1PlusStarw &&
                   NwStarCount.Equals(other.NwStarCount) &&
                   MostLikelies.SequenceEqual(other.MostLikelies) &&
                   TwoGrams.SequenceEqual(other.TwoGrams, new KeyValuePairIntTwoStringInfoComparer());
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var comparer = new KeyValuePairIntTwoStringInfoComparer();
                var hashCode = (int) OneGramCount;
                hashCode = (hashCode*397) ^ (int) N1PlusStarwStar;
                hashCode = (hashCode*397) ^ (int) N1PlusStarw;
                hashCode = (hashCode*397) ^ NwStarCount.GetHashCode();
                hashCode = MostLikelies?.Aggregate(hashCode, (current, count) => (current * 397) ^ count) ?? hashCode;
                hashCode = TwoGrams?.Aggregate(hashCode, (current, twoGram) => (current * 397) ^ comparer.GetHashCode(twoGram)) ?? hashCode;
                return hashCode;
            }
        }
    }
}
