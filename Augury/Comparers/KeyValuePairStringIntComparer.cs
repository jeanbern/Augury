using System.Collections.Generic;

namespace Augury.Comparers
{
    public class KeyValuePairStringIntComparer : IEqualityComparer<KeyValuePair<string, int>>
    {
        public bool Equals(KeyValuePair<string, int> x, KeyValuePair<string, int> y)
        {
            return Equals(x.Value, y.Value) && Equals(x.Key, y.Key);
        }

        public int GetHashCode(KeyValuePair<string, int> obj)
        {
            unchecked
            {
                return (obj.Value.GetHashCode() * 397) ^ base.GetHashCode();
            }
        }
    }
}