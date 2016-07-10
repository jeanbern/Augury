﻿using System;
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

            int low = 0, high = 0;
            var allNodes = new List<DawgNode>();
            var totalChildCount = root.Traversal(ref low, ref high, allNodes);
            Func<int, int> realIndex = x => -low + (x < 0 ? x : x - 1);

            Characters = allNodes.SelectMany(node => node.Children.Keys).Distinct().OrderBy(character => character).ToArray();
            Edges = new int[totalChildCount];
            EdgeCharacter = new ushort[totalChildCount];
            FirstChildIndex = new int[high - low];
            RootNodeIndex = realIndex(root.Id);
            TerminalCount = -low;

            var characterIndex = Characters.Select((character, i) => new KeyValuePair<char, ushort>(character, (ushort)i)).ToDictionary(x => x.Key, x => x.Value);
            var edgeIndex = 0;
            foreach (var node in allNodes)
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
    }
}
