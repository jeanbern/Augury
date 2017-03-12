using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Augury
{
    public class Dawg
    {
        internal readonly int TerminalCount;
        internal readonly int RootNodeIndex;
        internal readonly char[] Characters;
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

        internal Dawg(IEnumerable<string> words)
        {
            var builder = new DawgBuilder();
            foreach(var word in words)
            {
                builder.Insert(word);
            }

            var root = builder.Finish();

            var allNodes = new List<DawgNode>();
            var allChars = new HashSet<char>();
            int low = 0, high = 0;
            var totalChildCount = root.Traversal(ref low, ref high, allNodes, allChars);
            Func<int, int> realIndex = x => -low + (x < 0 ? x : x - 1);

            Characters = allChars.OrderBy(character => character).ToArray();
            Edges = new int[totalChildCount];
            EdgeCharacter = new ushort[totalChildCount];
            FirstChildIndex = new int[high - low];
            RootNodeIndex = realIndex(root.Id);
            TerminalCount = -low;

            var characterIndex = Characters.Select((character, i) => new KeyValuePair<char, ushort>(character, (ushort)i)).ToDictionary(x => x.Key, x => x.Value);

            var orderedNodes = new DawgNode[allNodes.Count];
            foreach (var node in allNodes)
            {
                orderedNodes[realIndex(node.Id)] = node;
            }

            var edgeIndex = 0;
            foreach (var node in orderedNodes)
            {
                FirstChildIndex[realIndex(node.Id)] = edgeIndex;
                foreach (var child in node.SortedChildren)
                {
                    Edges[edgeIndex] = realIndex(child.Value.Id);
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

        public override bool Equals(object obj)
        {
            var other = obj as Dawg;
            return other != null && Equals(other);
        }

        protected bool Equals(Dawg other)
        {
            return TerminalCount == other.TerminalCount &&
                   RootNodeIndex == other.RootNodeIndex &&
                   Characters.SequenceEqual(other.Characters) &&
                   FirstChildIndex.SequenceEqual(other.FirstChildIndex) &&
                   Edges.SequenceEqual(other.Edges) &&
                   EdgeCharacter.SequenceEqual(other.EdgeCharacter);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = TerminalCount;
                hashCode = (hashCode * 397) ^ RootNodeIndex;
                hashCode = Characters?.Aggregate(hashCode, (current, character) => (current * 397) ^ character) ?? hashCode;
                hashCode = FirstChildIndex?.Aggregate(hashCode, (current, childIndex) => (current * 397) ^ childIndex) ?? hashCode;
                hashCode = Edges?.Aggregate(hashCode, (current, edge) => (current * 397) ^ edge) ?? hashCode;
                hashCode = EdgeCharacter?.Aggregate(hashCode, (current, edgeCharacter) => (current * 397) ^ edgeCharacter) ?? hashCode;
                return hashCode;
            }
        }
    }
}
