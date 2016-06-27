using Augury.Base;
using Augury.Lucene;
using Augury.PriorityQueue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Augury
{
    public class SpellCheck : Dawg
    {
        /// <summary>
        /// Finds possible corrections or extensions for a given word.
        /// </summary>
        /// <param name="prefix">The word to spell-check.</param>
        /// <param name="maxResults">The maximum number of results returned.</param>
        /// <returns>Up to <see cref="maxResults">maxResults</see> words along with their similarity score. Not sorted.</returns>
        public IEnumerable<IWordSimilarityNode> PrefixLookup(string prefix, int maxResults)
        {
            var builder = new StringBuilder();
            var startNode = RootNodeIndex;
            if (prefix.Length > 5)
            {
                var cheatIndex = (prefix.Length - 1) / 2;
                var cheatString = prefix.Substring(0, cheatIndex);
                builder.Append(cheatString);
                startNode = GetNodeForString(cheatString);
                if (startNode == -1)
                {
                    //With a long string, we need at least part of it to match or there would be too many hits.
                    return new List<IWordSimilarityNode>();
                }

                prefix = prefix.Substring(cheatIndex);
            }

            var maxErrors = prefix.Length > 4 ? 2 : prefix.Length > 2 ? 1 : 0;

            var possibleWords = new HashSet<string>();
            FindCorrections(possibleWords, builder, prefix, startNode, maxErrors, 0, 0);
            return OrderPossibilities(prefix, maxResults, possibleWords);
        }

        private static IEnumerable<IWordSimilarityNode> OrderPossibilities(string input, int maxResults, ICollection<string> possibleResults)
        {
            //If we only have a small amount, we can sort them all.
            if (possibleResults.Count <= maxResults)
            {
                var queue = new WordQueue(possibleResults.Count);
                foreach (var possibleResult in possibleResults)
                {
                    queue.Enqueue(possibleResult, JaroWinkler.BoundedSimilarity(input, possibleResult));
                }

                return queue.OrderByDescending(x => x.Similarity);
            }

            var max = Math.Min(possibleResults.Count, maxResults);

            //With a large number of words to evaluate, but we only care about the top matches.
            //Instead of sorting a large number of elements at the end, we use a queue and restrict it's running size to max.
            //This queue will do that faster than other container types.
            //SortedList.RemoveAt is O(n)
            //SortedDictionary/SortedSet.ElementAt is O(n)
            var likelyWordsQueue = new WordQueue(max);
            foreach (var word in possibleResults)
            {
                var jw = JaroWinkler.BoundedSimilarity(input, word);
                likelyWordsQueue.Enqueue(word, jw);
            }

            return likelyWordsQueue.OrderByDescending(x => x.Similarity);
        }

        private void FindCorrections(HashSet<string> soFar, StringBuilder builder, string prefix, int node, int maxErrors, int match, int transpositions)
        {
            if (maxErrors < 0 || node == -1)
            {
                return;
            }

            if (prefix.Length == 0)
            {
                if (match == 0)
                {
                    return;
                }

                //Note: This is not a perfect Jaro-Winkler match #
                //It does not take into account the distance between the matching indices.
                //But we don't care, proper values will be computed later.
                //This is pre-processing to avoid values that could never match.
                var maxJaroLength = JaroWinkler.MaxLengthForPrefix(match, builder.Length, transpositions);
                MatchPrefix(soFar, builder, node, maxJaroLength - builder.Length);
                return;
            }

            var want = prefix.First();
            var firstChildIndex = FirstChildIndex[node];
            var lastChildIndex = node + 1 < FirstChildIndex.Length ? FirstChildIndex[node + 1] : Edges.Length;

            for (var i = firstChildIndex; i < lastChildIndex; i++)
            {
                var found = Characters[EdgeCharacter[i]];
                var newNode = Edges[i];

                builder.Append(found);

                //If possible, Try as if it's the right character
                if (want == found)
                {
                    FindCorrections(soFar, builder, prefix.Substring(1), newNode, maxErrors, match + 1, transpositions);
                }

                //Try as if the character is an insertion
                FindCorrections(soFar, builder, prefix, newNode, maxErrors - 1, match, transpositions);

                //Try as if the character is a deletion
                if (prefix.Length > 1)
                {
                    FindCorrections(soFar, builder, prefix.Substring(2), node, maxErrors - 1, match, transpositions);
                }

                //Try as if there's a substitution
                FindCorrections(soFar, builder, prefix.Substring(1), newNode, maxErrors - 1, match, transpositions);

                //Try as if there's a transposition
                WasTransposition(soFar, builder, prefix.Substring(1), newNode, maxErrors - 1, want, found, match, transpositions + 1);

                builder.Length--;
            }
        }

        private void WasTransposition(HashSet<string> soFar, StringBuilder builder, string prefix, int node, int maxErrors, char previousWant, char previousFound, int match, int transpositions)
        {
            if (maxErrors < 0 || node == -1 || prefix.Length == 0)
            {
                return;
            }

            var want = prefix.First();
            var firstChildIndex = FirstChildIndex[node];
            var lastChildIndex = node + 1 < FirstChildIndex.Length ? FirstChildIndex[node + 1] : Edges.Length;

            for (var i = firstChildIndex; i < lastChildIndex; i++)
            {
                var found = Characters[EdgeCharacter[i]];

                builder.Append(found);
                var newNode = Edges[i];

                if (want == previousFound && previousWant == found)
                {
                    FindCorrections(soFar, builder, prefix.Substring(1), newNode, maxErrors, match /* + 1*/, transpositions);
                }
                else
                {
                    if (want == found)
                    {
                        WasTransposition(soFar, builder, prefix.Substring(1), newNode, maxErrors, previousWant, previousFound, match + 1, transpositions);
                    }

                    //Try as if the character is an insertion
                    WasTransposition(soFar, builder, prefix, newNode, maxErrors - 1, previousWant, previousFound, match, transpositions);

                    //Try as if the character is a deletion
                    if (prefix.Length > 1)
                    {
                        WasTransposition(soFar, builder, prefix.Substring(2), node, maxErrors - 1, previousWant, previousFound, match, transpositions);
                    }

                    //Try as if there's a substitution
                    WasTransposition(soFar, builder, prefix.Substring(1), newNode, maxErrors - 1, previousWant, previousFound, match, transpositions);

                    //Try as if there's a transposition
                    WasTransposition(soFar, builder, prefix.Substring(1), newNode, maxErrors - 1, previousWant, previousFound, match, transpositions);
                }

                builder.Length--;
            }
        }



        public SpellCheck(int terminalCount, char[] characters, int rootNodeIndex, int[] firstChildForNode, int[] edges, ushort[] edgeCharacter)
            : base(terminalCount, characters, rootNodeIndex, firstChildForNode, edges, edgeCharacter)
        {
        }

        internal SpellCheck(List<string> root) : base(root)
        {
        }
    }
}
