using System.Collections.Generic;

namespace Augury.Comparers
{
    public class KeyValuePairIntUintComparer : IEqualityComparer<KeyValuePair<int, uint>>
    {
        public bool Equals(KeyValuePair<int, uint> x, KeyValuePair<int, uint> y)
        {
            return Equals(x.Value, y.Value) && Equals(x.Key, y.Key);
        }

        public int GetHashCode(KeyValuePair<int, uint> obj)
        {
            unchecked
            {
                return (obj.Value.GetHashCode() * 397) ^ base.GetHashCode();
            }
        }
    }
}