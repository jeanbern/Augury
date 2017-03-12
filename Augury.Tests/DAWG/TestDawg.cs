using System.Collections.Generic;

namespace Augury.Test.DAWG
{
    public class TestDawg : Dawg, ITestDawg
    {
        internal TestDawg(int terminalCount, char[] characters, int rootNodeIndex, int[] firstChildForNode, int[] edges, ushort[] edgeCharacter) : base(terminalCount, characters, rootNodeIndex, firstChildForNode, edges, edgeCharacter)
        {
        }

        public TestDawg(IEnumerable<string> words) : base(words)
        {
        }

        public int EdgeCount => Edges.Length;

        public int NodeCount => FirstChildIndex.Length;
    }
}
