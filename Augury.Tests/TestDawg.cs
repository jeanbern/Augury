using System.Collections.Generic;

namespace Augury.Test
{
    public class TestDawg : Dawg
    {
        internal TestDawg(int terminalCount, char[] characters, int rootNodeIndex, int[] firstChildForNode, int[] edges, ushort[] edgeCharacter) : base(terminalCount, characters, rootNodeIndex, firstChildForNode, edges, edgeCharacter)
        {
        }

        public TestDawg(List<string> words) : base(words)
        {
        }

        public int EdgeCount { get { return Edges.Length; } }
        public int NodeCount { get { return FirstChildIndex.Length; } }
    }
}
