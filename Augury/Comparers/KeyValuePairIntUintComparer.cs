using System;
using System.Collections.Generic;

namespace Augury.Comparers
{
    public class KeyValuePairIntUintComparer : IEqualityComparer<KeyValuePair<int, uint>>
    {
        public bool Equals(KeyValuePair<int, uint> x, KeyValuePair<int, uint> y)
        {
            throw new NotImplementedException();
        }

        public int GetHashCode(KeyValuePair<int, uint> obj)
        {
            throw new NotImplementedException();
        }
    }
}
