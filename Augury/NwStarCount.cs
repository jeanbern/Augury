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
    }
}
