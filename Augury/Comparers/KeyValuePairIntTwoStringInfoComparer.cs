using System.Collections.Generic;

namespace Augury.Comparers
{
    internal class KeyValuePairIntTwoStringInfoComparer : IEqualityComparer<KeyValuePair<int, TwoStringInfo>>
    {
        public bool Equals(KeyValuePair<int, TwoStringInfo> x, KeyValuePair<int, TwoStringInfo> y)
        {
            return Equals(x.Key, y.Key) && x.Value.Equals(y.Value);
        }

        public int GetHashCode(KeyValuePair<int, TwoStringInfo> obj)
        {
            unchecked
            {
                return (obj.Value.GetHashCode() * 397) ^ base.GetHashCode();
            }
        }
    }
}