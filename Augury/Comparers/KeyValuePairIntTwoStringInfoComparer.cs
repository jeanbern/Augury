using System;
using System.Collections.Generic;

namespace Augury.Comparers
{
    class KeyValuePairIntTwoStringInfoComparer : IEqualityComparer<KeyValuePair<int, TwoStringInfo>>
    {
        public bool Equals(KeyValuePair<int, TwoStringInfo> x, KeyValuePair<int, TwoStringInfo> y)
        {
            //TODO
            throw new NotImplementedException();
        }

        public int GetHashCode(KeyValuePair<int, TwoStringInfo> obj)
        {
            //TODO
            throw new NotImplementedException();
        }
    }
}
