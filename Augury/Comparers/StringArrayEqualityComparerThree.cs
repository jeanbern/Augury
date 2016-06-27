using System.Collections.Generic;

namespace Augury.Comparers
{
    public class StringArrayEqualityComparerThree : IEqualityComparer<string[]>
    {
        public bool Equals(string[] x, string[] y)
        {
            return x[0] == y[0] && x[1] == y[1] && x[2] == y[2];
        }

        public int GetHashCode(string[] obj)
        {
            unchecked
            {
                return obj[0].GetHashCode() * 31 + 17 * obj[1].GetHashCode() + 17 * obj[2].GetHashCode();
            }
        }
    }
}
