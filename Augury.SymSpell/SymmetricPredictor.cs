using Augury.Base;
using Augury.Lucene;
using Augury.PriorityQueue;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Augury.SymSpell
{
    /// <summary>
    /// Thanks to the IsRealWord bool, we can ensure no duplicate entries in WordList.
    /// Other solutions require either a secondary Dictionary/HashSet/SortedList which contains the exact same strings as WordList.
    /// This saves space and time.
    /// </summary>
    public class SymmetricEntry
    {
        public bool IsRealWord;
        public List<int> Indices;
    }

    /// <see cref="http://github.com/jeanbern/symspell">
    /// Maintained in a branch to comply with licensing terms.
    /// </see>
    public sealed class SymmetricPredictor
    {

        /// <summary>
        /// Finds possible corrections or extensions for a given word.
        /// </summary>
        /// <param name="input">The word to spell-check.</param>
        /// <param name="maxResults">The maximum number of results returned.</param>
        /// <returns>Up to <see cref="maxResults">maxResults</see> words along with their similarity score. Not sorted.</returns>
        public IEnumerable<IWordSimilarityNode> PrefixLookup(string input, int maxResults)
        {
            var iLen = input.Length;
            ICollection<string> possibleResults;

            if (iLen < 3)
            {
                SymmetricEntry di;
                if (!Dictionary.TryGetValue(input, out di))
                {
                    return new WordSimilarityNode[0];
                }

                possibleResults = di.Indices.Select(index => Wordlist[index]).Where(word => word.Length >= iLen).ToList();
            }
            else
            {
                possibleResults = new HashSet<string>();
                var deletes = new HashSet<string> { input };
                //Arbitray decision of the allowable number of deletes.
                //Could have this start smaller and increase it if a threshold is not met.
                //But then we risk the problem of having a prefix with a lot of very close but uncommon words.
                //A small deletion count could miss a word with slightly more distance, but that is much more common.
                //I don't even know if those exist though.
                Edits(input, (iLen - 1) / 3, deletes);
                foreach (var delete in deletes)
                {
                    SymmetricEntry di;
                    if (!Dictionary.TryGetValue(delete, out di)) { continue; }
                    foreach (var word in di.Indices.Select(index => Wordlist[index]).Where(word => word.Length >= iLen))
                    {
                        possibleResults.Add(word);
                    }
                }
            }

            SymmetricEntry se;
            if (Dictionary.TryGetValue(input, out se) && se.IsRealWord)
            {
                possibleResults.Add(input);
            }

            //If we only have a small amount, we can sort them all.
            if (possibleResults.Count <= maxResults)
            {
                return possibleResults.Select(word => new WordSimilarityNode { Word = word, Similarity = JaroWinkler.BoundedSimilarity(input, word) });
            }

            var max = Math.Min(possibleResults.Count, maxResults);

            //With a large number of words to evaluate, but we only care about the top matches.
            //Instead of sorting a large number of elements at the end, we use a queue and restrict it's running size to max.
            //This queue will do that faster than other container types.
            //SortedList.RemoveAt is O(n)
            //SortedDictionary/SortedSet.ElementAt is O(n)
            var likelyWordsQueue = new WordQueue(max);
            var count = 0;
            foreach (var word in possibleResults)
            {
                var jw = JaroWinkler.BoundedSimilarity(input, word);
                if (count < max)
                {
                    ++count;
                    likelyWordsQueue.Enqueue(word, jw);
                }
                else
                {
                    if (jw < likelyWordsQueue.First.Similarity) { continue; }
                    likelyWordsQueue.Dequeue();
                    likelyWordsQueue.Enqueue(word, jw);
                }
            }

            return likelyWordsQueue;
        }

        /// <summary>
        /// Constructor for use by the serializer, you should be using this otherwise.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="wordlist"></param>
        internal SymmetricPredictor(Dictionary<string, SymmetricEntry> dictionary, List<string> wordlist)
        {
            Dictionary = dictionary;
            Wordlist = wordlist;
        }

        /// <summary>
        /// Will process a list of words into the Dictionary and WordList
        /// </summary>
        /// <param name="strings">The words in the corpus.</param>
        public SymmetricPredictor(IEnumerable<string> strings)
        {
            foreach (var key in strings)
            {
                CreateSymmetricEntry(key);
            }

            Wordlist.TrimExcess();
        }

        public void AddWordsNoSpelling(IEnumerable<string> strings)
        {
            foreach (var key in strings)
            {
                SymmetricEntry value;
                if (Dictionary.TryGetValue(key, out value))
                {
                    if (value.IsRealWord)
                    {
                        //Already processed it.
                        continue;
                    }

                    value.IsRealWord = true;
                }
                else
                {
                    value = new SymmetricEntry { IsRealWord = true, Indices = new List<int>() };
                    Dictionary.Add(key, value);
                }

                Wordlist.Add(key);
            }
        }

        /// <summary>
        /// The Dictionary stores values in the following way:
        /// (key: prefix or misspelling, value: {int})
        /// The key is a string, it can be a valid word, a misspelling of a word, or a prefix (misspelt or not) of a word.
        /// The values represent indices in the WordList, each of these correspond to a word which has they key as either a prefix (spelled correctly or not), a mispelling, or as itself in the case of a proper word.
        /// </summary>
        internal readonly Dictionary<string, SymmetricEntry> Dictionary = new Dictionary<string, SymmetricEntry>();

        /// <summary>
        /// Stores all known words. This saves space in the dictionary by using int (4 byte) to point to each string, instead of duplicating the string itself.
        /// </summary>
        internal readonly List<string> Wordlist = new List<string>();

        /// <summary>
        /// Adds a word to the WordList. Also finds it's variations and inserts pointer back to itself in their dictionary entries.
        /// </summary>
        /// <param name="key">The new word.</param>
        internal void CreateSymmetricEntry(string key)
        {
            SymmetricEntry value;
            if (Dictionary.TryGetValue(key, out value))
            {
                if (value.IsRealWord)
                {
                    //Already processed it.
                    return;
                }

                value.IsRealWord = true;
            }
            else
            {
                value = new SymmetricEntry { IsRealWord = true, Indices = new List<int>() };
                Dictionary.Add(key, value);
            }

            Wordlist.Add(key);
            var keyint = Wordlist.Count - 1;

            var edits = PrefixesAndDeletes(key);
            foreach (var delete in edits)
            {
                SymmetricEntry di;
                if (Dictionary.TryGetValue(delete, out di))
                {
                    di.Indices.Add(keyint);
                }
                else
                {
                    di = new SymmetricEntry { Indices = new List<int> { keyint } };
                    Dictionary.Add(delete, di);
                }
            }
        }

        /// <summary>
        /// Find possible variations of a word and it's prefixes.
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        private static IEnumerable<string> PrefixesAndDeletes(string word)
        {
            var wLen = word.Length;

            //Only use HashSets when there's a chance of duplicates, otherwise save time by using a normal list.
            switch (wLen)
            {
                case 0:
                    return new List<string>();
                case 1:
                    return new List<string>();
                case 2:
                    return new List<string> { word[0].ToString(CultureInfo.InvariantCulture) };
                case 3:
                    var deletes3 = new HashSet<string> { word[0].ToString(CultureInfo.InvariantCulture) };
                    Edits(word, 1, deletes3);
                    return deletes3;
                case 4:
                    var deletes4 = new HashSet<string> { word[0].ToString(CultureInfo.InvariantCulture) };
                    Edits(word.Substring(0, 3), 1, deletes4);
                    Edits(word, 1, deletes4);
                    return deletes4;
                default:
                    var deletes = new HashSet<string> { word[0].ToString(CultureInfo.InvariantCulture) };
                    Edits(word.Substring(0, 3), 1, deletes);
                    Edits(word.Substring(0, 4), 1, deletes);

                    for (var x = wLen; x > 4; x--)
                    {
                        Edits(word.Substring(0, x), 2, deletes);
                    }

                    return deletes;
            }
        }

        /// <summary>
        /// Find all possible variations of a word, with up to <see cref="editDistanceRemaining"/> deletes.
        /// </summary>
        /// <param name="word">The word from which to remove letters.</param>
        /// <param name="editDistanceRemaining">The number of letters we can remove.</param>
        /// <param name="deletes">The set of deletes so far.</param>
        /// <remarks></remarks>
        private static void Edits(string word, int editDistanceRemaining, HashSet<string> deletes)
        {
            //Put in this order to normalize runtime. Probably doesn't do anything in reality.
            switch (editDistanceRemaining)
            {
                default:
                    var wLen = word.Length;
                    var newDistance = editDistanceRemaining - 1;
                    for (var i = 0; i < wLen; i++)
                    {
                        var delete = word.Remove(i, 1);
                        if (deletes.Add(delete))
                        {
                            Edits(delete, newDistance, deletes);
                        }
                    }
                    return;
                case 1:
                    var wordLen = word.Length;
                    for (var i = 0; i < wordLen; i++)
                    {
                        var delete = word.Remove(i, 1);
                        deletes.Add(delete);
                    }

                    return;
                case 0:
                    return;
            }
        }
    }
}