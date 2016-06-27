using System.Collections.Generic;

namespace Augury.Comparers
{
    public class StringArrayEqualityComparerTwo : IEqualityComparer<string[]>
    {
        public bool Equals(string[] x, string[] y)
        {
            return x[0] == y[0] && x[1] == y[1];
        }

        public int GetHashCode(string[] obj)
        {
            unchecked
            {
                return obj[0].GetHashCode() * 31 + 17 * obj[1].GetHashCode();
            }
        }
    }
}
