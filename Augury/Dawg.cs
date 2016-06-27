using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Augury
{
    public class Dawg
    {
        internal readonly int TerminalCount;
        internal readonly char[] Characters;
        internal readonly int RootNodeIndex;
        internal readonly int[] FirstChildIndex;

        internal readonly int[] Edges;
        internal readonly ushort[] EdgeCharacter;

        protected Dawg(int terminalCount, char[] characters,
            int rootNodeIndex, int[] firstChildForNode,
            int[] edges, ushort[] edgeCharacter)
        {
            TerminalCount = terminalCount;
            Characters = characters;
            RootNodeIndex = rootNodeIndex;
            FirstChildIndex = firstChildForNode;
            Edges = edges;
            EdgeCharacter = edgeCharacter;
        }

        protected Dawg(List<string> words)
        {
            var builder = new DawgBuilder();
            words.ForEach(builder.Insert);
            var root = builder.Finish();

            var allNodes = root.GetNodes().ToArray();
            var terminalNodes = allNodes.Where(x => x.TerminalNode).ToArray();
            TerminalCount = terminalNodes.Count();
            allNodes = terminalNodes.Concat(allNodes.Where(x => !x.TerminalNode)).ToArray();

            Characters = allNodes.SelectMany(node => node.Children.Keys).Distinct().OrderBy(character => character).ToArray();
            var characterIndex = Characters.Select((character, i) => new KeyValuePair<char, ushort>(character, (ushort) i)).ToDictionary(x => x.Key, x => x.Value);
            var totalChildCount = allNodes.Sum(n => n.Children.Count);

            var nodeIndex = allNodes.Select((node, i) => new KeyValuePair<DawgNode, int>(node, i)).ToDictionary(x => x.Key, x => x.Value);
            if (!nodeIndex.TryGetValue(root, out RootNodeIndex))
            {
                RootNodeIndex = -1;
            }

            FirstChildIndex = new int[allNodes.Length];
            Edges = new int[totalChildCount];
            EdgeCharacter = new ushort[totalChildCount];

            var edgeIndex = 0;
            foreach (var node in nodeIndex)
            {
                FirstChildIndex[node.Value] = edgeIndex;
                foreach (var child in node.Key.SortedChildren)
                {
                    Edges[edgeIndex] = nodeIndex[child.Value];
                    EdgeCharacter[edgeIndex] = characterIndex[child.Key];
                    edgeIndex++;
                }
            }
        }

        protected int GetNodeForString(string prefix)
        {
            var node = RootNodeIndex;
            foreach (var target in prefix)
            {
                var firstChildIndex = FirstChildIndex[node];
                var lastChildIndex = node + 1 < FirstChildIndex.Length ? FirstChildIndex[node + 1] : Edges.Length;
                node = -1;
                
                for (var i = firstChildIndex; i < lastChildIndex; i++)
                {
                    if (target != Characters[EdgeCharacter[i]]) {continue;}

                    node = Edges[i];
                    break;
                }

                if (node == -1)
                {
                    return -1;
                }
            }

            return node;
        }

        public ICollection<string> AllWords()
        {
            var words = new List<string>();
            MatchPrefix(words, new StringBuilder(), RootNodeIndex, -1);
            return words;
        }

        protected void MatchPrefix(ICollection<string> found, StringBuilder sb, int nodeIndex, int maxDepth)
        {
            if (nodeIndex == -1)
            {
                return;
            }

            if (nodeIndex < TerminalCount)
            {
                found.Add(sb.ToString());
            }

            if (maxDepth == 0)
            {
                return;
            }

            var firstChildIndex = FirstChildIndex[nodeIndex];
            var lastChildIndex = nodeIndex + 1 < FirstChildIndex.Length ? FirstChildIndex[nodeIndex + 1] : Edges.Length;
            for (var i = firstChildIndex; i < lastChildIndex; i++)
            {
                sb.Append(Characters[EdgeCharacter[i]]);
                MatchPrefix(found, sb, Edges[i], maxDepth - 1);
                sb.Length--;
            }
        }
    }
}
