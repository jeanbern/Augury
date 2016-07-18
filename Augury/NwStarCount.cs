namespace Augury
{
    internal struct NwStarCount
    {
        public static bool Equals(NwStarCount one, NwStarCount two)
        {
            return one.N1WStar == two.N1WStar && one.N2WStar == two.N2WStar && one.N3PlusWStar == two.N3PlusWStar;
        }

        public uint N1WStar;
        public uint N2WStar;
        public uint N3PlusWStar;
        public override bool Equals(object obj)
        {
            if (obj is NwStarCount)
            {
                return Equals((NwStarCount) obj);
            }

            return base.Equals(obj);
        }

        public bool Equals(NwStarCount other)
        {
            return N1WStar == other.N1WStar &&
                   N2WStar == other.N2WStar &&
                   N3PlusWStar == other.N3PlusWStar;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) N1WStar;
                hashCode = (hashCode*397) ^ (int) N2WStar;
                hashCode = (hashCode*397) ^ (int) N3PlusWStar;
                return hashCode;
            }
        }
    }
}
