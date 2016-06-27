﻿using Augury.Lucene;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Augury
{
    /// <summary>
    /// Provides methods for calculating the probability of a word appearing based on words preceeding it. Uses the modified Kneser-Ney algorithm from Chen & Goodman 1999.
    /// </summary>
    /// <see cref="http://www2.denizyuret.com/ref/goodman/chen-goodman-99.pdf">Chen & Goodman 1999. The paper detailing the algorithm used here to calculate predictions.</see>
    /// <seealso cref="http://ieeexplore.ieee.org/xpl/login.jsp?tp=&arnumber=479394">Knesser & Ney 1995. The paper referenced by Chen & Goodman</seealso>
    /// <seealso cref="http://west.uni-koblenz.de/sites/default/files/BachelorArbeit_MartinKoerner.pdf">Martin Christian Körner 2013. For more readable notation.</seealso>
    public class Auger
    {
        public bool English { get; set; }

        #region predict

        //The larger this value, the more words we check for history probability.
        //We maintain a sorted list with this length, it could use a lot of processing power.
        private const int MaxSpellCheckResults = 100;
        //This does not change the amount of time it takes to process results.
        private const int MaxResults = 10;

        /// <summary>
        /// Generates predictions for the next word.
        /// </summary>
        /// <param name="history">The text so far. Lower-cased as appropriate.</param>
        /// <returns>
        /// Predictions in descending order of likelyhood.
        /// Predictions are generated via the Modified Kneser-Ney algorithm.
        /// </returns>
        /// <remarks>
        /// If the last string in history is empty, it will attempt to predict entirely new words.
        /// If the last string is not empty, it will use that as the basis for a spell-checked prediction.
        /// The <see cref="SpellCheck">spell-checker</see> will attempt to provide words which are either close to the provided word, or contain something close as a prefix.
        /// Closeness of two words is defined by the <see cref="JaroWinkler">Jaro-Winkler</see> distance.
        /// This distance is estimated for words values below a threshold.
        /// </remarks>
        public IReadOnlyList<string> Predict(IReadOnlyList<string> history)
        {
            var lastWord = history[history.Count - 1];
            history = history.Select(x => x.ToLowerInvariant()).ToList();
            var hLen = history.Count;
            //Only care about the last 2, since we deal with 3-grams
            string[] previous;
            string partial;
            switch (hLen)
            {
                case 0:
                    return new List<string>();
                case 1:
                    //return SpellingPredictor.PrefixLookup(history[0], MaxSpellCheckResults).OrderByDescending(x => x.Similarity).Take(MaxResults).Select(x => x.Word).ToList();
                    partial = history[0];
                    previous = new string[0];
                    break;
                case 2:
                    partial = history[1];
                    previous = new[] {history[0]};
                    break;
                case 3:
                    partial = history[2];
                    previous = new[] {history[0], history[1]};
                    break;
                default:
                    partial = history[hLen - 1];
                    previous = new[] {history[hLen - 3], history[hLen - 2]};
                    break;
            }

            if (string.IsNullOrWhiteSpace(partial))
            {
                return MostLikelySearch(previous);
            }

            //This is a (potentially) really huge list of words that are obtained via correcting and/or starting with partial.
            //Does not care about the word history.
            var spellchecks = SpellChecker.PrefixLookup(partial, MaxSpellCheckResults);

            var possibleSpellchecks = new Dictionary<string, double>();
            //Now we take our possible spell-checked words and see if any of them fit well with the history.
            foreach (var spellcheckPair in spellchecks)
            {
                var spellcheck = spellcheckPair.Word;
                var distance = spellcheckPair.Similarity;
                var newList = previous.ToList();
                newList.Add(spellcheck);
                var value = GetP(newList.ToArray());

                possibleSpellchecks.Add(spellcheck, value > 0 ? value*distance : distance - 1);
            }

            var ret = possibleSpellchecks.OrderByDescending(x => x.Value).Select(x => x.Key).Take(MaxResults).ToList();

            if (possibleSpellchecks.ContainsKey(partial))
            {
                if (ret.Contains(partial))
                {
                    ret.Remove(partial);
                }
                else
                {
                    ret.RemoveAt(ret.Count - 1);
                }

                if (ret.Count > 0)
                {
                    ret.Insert(0, partial);
                }
                else
                {
                    ret.Add(partial);
                }
            }

            return ret.Select(x => Tokenizer.CapitalizeFromTemplate(lastWord, x, hLen == 1)).ToList();
        }

        private IReadOnlyList<string> MostLikelySearch(string[] previous)
        {
            //TODO: do I need to return a list with an empty string? or is an empty list better?
            //There is no partial word, we can't spellcheck at all.
            //We also don't really want to itterate all n-grams that start with previous, that would take a looong time. 
            //So we have a premade lookup table of common ones.
            if (previous.Length == 0)
            {
                return new List<string> {""};
            }

            int oneIndex;
            if (!ReverseWordList.TryGetValue(previous[0], out oneIndex))
            {
                //The first word isn't a real word
                if (previous.Length == 1)
                {
                    return new List<string> {""};
                }

                int secondStringIndex;
                if (ReverseWordList.TryGetValue(previous[1], out secondStringIndex))
                {
                    //The second word is, so let's use words that follow it 
                    var secondStringAsFirst = DataSet[secondStringIndex];
                    var results = secondStringAsFirst.MostLikelies.Select(x => ReverseWordList[x]).ToList();
                    return results;
                }

                return new List<string> {""};
            }

            var oneInfo = DataSet[oneIndex];
            if (previous.Length == 1)
            {
                var results = oneInfo.MostLikelies.Select(x => ReverseWordList[x]).ToList();
                return results;
            }

            int twoIndex;
            if (!ReverseWordList.TryGetValue(previous[1], out twoIndex))
            {
                return new List<string> {""};
            }

            TwoStringInfo twoInfo;
            if (!oneInfo.TwoGrams.TryGetValue(twoIndex, out twoInfo) || twoInfo.MostLikelies.Count == 0)
            {
                //The second word doesn't often come after the first, let's try just using the second.
                int secondStringIndex;
                if (ReverseWordList.TryGetValue(previous[1], out secondStringIndex))
                {
                    var secondStringAsFirst = DataSet[secondStringIndex];
                    var results = secondStringAsFirst.MostLikelies.Select(x => ReverseWordList[x]).ToList();
                    return results;
                }

                return new List<string> {""};
            }

            var actualThreeGramResultsWow = twoInfo.MostLikelies.Select(x => ReverseWordList[x]).ToList();
            return actualThreeGramResultsWow;
        }

        #endregion

        internal uint N11, N12, N13, N14;
        internal uint N21, N22, N23, N24;

        private double _d11, _d12, _d13;
        private double _d21, _d22, _d23;

        internal readonly double TwoGramCount;

        internal readonly OrderedDictionary<string, int> ReverseWordList = new OrderedDictionary<string, int>();

        internal List<OneStringInfo> DataSet = new List<OneStringInfo>();

        public SpellCheck SpellChecker
        {
            get { return _spellChecker; }
        }

        private readonly SpellCheck _spellChecker;

        private const int Threshold = 10;
        private const int TwoGramThreshold = 5;
        private const int ThreeGramThreshold = 3;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="threeGrams"></param>
        /// <param name="twoGrams"></param>
        /// <param name="oneGrams"></param>
        /// <param name="words"></param>
        internal Auger(IEnumerable<KeyValuePair<string[], uint>> threeGrams, IEnumerable<KeyValuePair<string[], uint>> twoGrams, Dictionary<string, uint> oneGrams, List<string> words)
        {
            var rejectedOneGrams = new HashSet<string>();
            var keys = oneGrams.Keys.ToList();
            foreach (var key in keys.Where(key => oneGrams[key] < Threshold))
            {
                oneGrams.Remove(key);
                rejectedOneGrams.Add(key);
            }

            TwoGramCount = 0.0;
            words.Sort(StringComparer.OrdinalIgnoreCase);
            _spellChecker = new SpellCheck(words);

            var x = 0;
            foreach (var oneGram in oneGrams)
            {
                ReverseWordList.Add(oneGram.Key, x);
                DataSet.Add(new OneStringInfo {OneGramCount = oneGram.Value});
                ++x;
            }

            foreach (var twoGram in twoGrams)
            {
                var first = twoGram.Key[0];
                var second = twoGram.Key[1];
                if (rejectedOneGrams.Contains(first) || rejectedOneGrams.Contains(second))
                {
                    continue;
                }

                TwoGramCount++;
                try
                {
                    var oneInfo = DataSet[ReverseWordList[first]];
                    switch (twoGram.Value)
                    {
                        case 1:
                            ++oneInfo.NwStarCount.N1WStar;
                            ++N21;
                            break;
                        case 2:
                            ++oneInfo.NwStarCount.N2WStar;
                            ++N22;
                            break;
                        case 3:
                            ++oneInfo.NwStarCount.N3PlusWStar;
                            ++N23;
                            break;
                        case 4:
                            ++oneInfo.NwStarCount.N3PlusWStar;
                            ++N24;
                            break;
                        default:
                            ++oneInfo.NwStarCount.N3PlusWStar;
                            break;
                    }

                    var secondOneInfo = DataSet[ReverseWordList[second]];
                    ++secondOneInfo.N1PlusStarw;

                    if (twoGram.Value < TwoGramThreshold)
                    {
                        continue;
                    }
                    oneInfo.TwoGrams.Add(ReverseWordList[second], new TwoStringInfo {TwoGramCount = twoGram.Value});
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            foreach (var threeGram in threeGrams)
            {
                var first = threeGram.Key[0];
                var second = threeGram.Key[1];
                var third = threeGram.Key[2];
                if (rejectedOneGrams.Contains(first) || rejectedOneGrams.Contains(second) || rejectedOneGrams.Contains(third))
                {
                    continue;
                }

                var oneInfo = DataSet[ReverseWordList[first]];

                var secondOneInfo = DataSet[ReverseWordList[second]];
                ++secondOneInfo.N1PlusStarwStar;

                TwoStringInfo twoInfo;
                TwoStringInfo secondTwoInfo;
                if (!oneInfo.TwoGrams.TryGetValue(ReverseWordList[second], out twoInfo))
                {
                    if (secondOneInfo.TwoGrams.TryGetValue(ReverseWordList[third], out secondTwoInfo))
                    {
                        ++secondTwoInfo.N1PlusStarww;
                    }
                    continue;
                }

                switch (threeGram.Value)
                {
                    case 1:
                        ++twoInfo.NwwStarCount.N1WStar;
                        ++N11;
                        break;
                    case 2:
                        ++twoInfo.NwwStarCount.N2WStar;
                        ++N12;
                        break;
                    case 3:
                        ++twoInfo.NwwStarCount.N3PlusWStar;
                        ++N13;
                        break;
                    case 4:
                        ++twoInfo.NwwStarCount.N3PlusWStar;
                        ++N14;
                        break;
                    default:
                        ++twoInfo.NwwStarCount.N3PlusWStar;
                        break;
                }

                if (secondOneInfo.TwoGrams.TryGetValue(ReverseWordList[third], out secondTwoInfo))
                {
                    ++secondTwoInfo.N1PlusStarww;
                }

                if (threeGram.Value < ThreeGramThreshold)
                {
                    continue;
                }
                twoInfo.ThreeGramCounts.Add(ReverseWordList[third], threeGram.Value);
            }

            SetDs();

            CalculateMostLikelies();
        }

        /// <summary>
        /// Don't use this, it's only for the serializer. Seriously.
        /// </summary>
        internal Auger(List<OneStringInfo> dataSet,
            SpellCheck spellChecker, OrderedDictionary<string, int> reverseWords,
            uint n11, uint n12, uint n13, uint n14,
            uint n21, uint n22, uint n23, uint n24,
            double twoGramCount)
        {
            DataSet = dataSet;
            _spellChecker = spellChecker;
            ReverseWordList = reverseWords;

            N11 = n11;
            N12 = n12;
            N13 = n13;
            N14 = n14;
            N21 = n21;
            N22 = n22;
            N23 = n23;
            N24 = n24;
            TwoGramCount = twoGramCount;

            SetDs();
        }

        private void CalculateMostLikelies()
        {
            foreach (var dataPoint in DataSet)
            {
                var oneStringInfo = dataPoint;
                var tops = oneStringInfo.TwoGrams.Where(x => x.Value.TwoGramCount >= 5).OrderByDescending(x => x.Value.TwoGramCount).Select(x => x.Key).Take(Math.Min(3, oneStringInfo.TwoGrams.Count));
                oneStringInfo.MostLikelies = tops.ToArray();

                foreach (var twoGramData in oneStringInfo.TwoGrams)
                {
                    var twoStringInfo = twoGramData.Value;
                    var twoTops = twoStringInfo.ThreeGramCounts.Where(x => x.Value >= 5).OrderByDescending(x => x.Value).Select(x => x.Key).Take(Math.Min(3, twoStringInfo.ThreeGramCounts.Count));
                    twoStringInfo.MostLikelies = twoTops.ToArray();
                }
            }
        }

        public uint GetCount(string word)
        {
            int index;
            return ReverseWordList.TryGetValue(word, out index) ? DataSet[index].OneGramCount : 0;
        }

        #region Save

        /// <summary>
        /// Saves the Auger to a stream.
        /// </summary>
        public void Save(Stream stream)
        {
            new AugerSerializer().Serialize(stream, this);
        }

        /// <summary>
        /// Load an Auger from a stream.
        /// </summary>
        /// <returns>The Auger stored in the stream.</returns>
        public static Auger Load(Stream stream)
        {
            return new AugerSerializer().Deserialize(stream);
        }

        #endregion

        #region getP

        public double GetP(params string[] history)
        {
            //Save two checks if length >= 3
            //Shortens the time for the calls that would take the longest.
            if (history.Length < 3)
            {
                if (history.Length == 2)
                {
                    return GetPFromTwo(history);
                }
                if (history.Length == 1)
                {
                    int attempt;
                    if (!ReverseWordList.TryGetValue(history[0], out attempt))
                    {
                        return 0.0;
                    }

                    return DataSet[attempt].OneGramCount/(double) DataSet.Count;
                }
                if (history.Length == 0)
                {
                    return 0.0;
                }
            }

            var w = history.Skip(history.Length - 3).ToArray();

            int oneIndex;
            if (!ReverseWordList.TryGetValue(w[0], out oneIndex))
            {
                return 0.0;
            }

            var oneInfo = DataSet[oneIndex];
            int twoIndex;
            TwoStringInfo twoInfo;
            if (!ReverseWordList.TryGetValue(w[1], out twoIndex) || !oneInfo.TwoGrams.TryGetValue(twoIndex, out twoInfo))
            {
                //if we haven't seens firstTwo, there's no 3-gram that matches ww_ so yhigh will be 0
                //no ww_ means no www, so first will also be 0
                return 0.0;
            }

            var denom = twoInfo.TwoGramCount;
            var nwwStar = twoInfo.NwwStarCount;

            var yhigh = (_d11*nwwStar.N1WStar + _d12*nwwStar.N2WStar + _d13*nwwStar.N3PlusWStar)/denom;
            var lowerP = GetLowerP(w.Skip(1).ToArray());

            int threeIndex;
            uint count;
            if (!ReverseWordList.TryGetValue(w[2], out threeIndex) || !twoInfo.ThreeGramCounts.TryGetValue(threeIndex, out count))
            {
                //first is 0
                return yhigh*lowerP;
            }

            var first = Math.Max(0.0, count - D(count))/denom;
            var pokemon = first + yhigh*lowerP;
            return pokemon;
        }

        private double GetPFromTwo(string[] w)
        {
            int oneIndex;
            if (!ReverseWordList.TryGetValue(w[0], out oneIndex))
            {
                //no w means no w_ or ww
                return 0.0;
            }

            var oneInfo = DataSet[oneIndex];
            var denom = oneInfo.OneGramCount;
            var nwStar = oneInfo.NwStarCount;
            var yhigh = (_d21*nwStar.N1WStar + _d22*nwStar.N2WStar + _d23*nwStar.N3PlusWStar)/denom;
            var lowerP = GetLowerLowerP(w[1]);

            int twoIndex;
            TwoStringInfo twoInfo;
            if (!ReverseWordList.TryGetValue(w[1], out twoIndex) || !oneInfo.TwoGrams.TryGetValue(twoIndex, out twoInfo))
            {
                //first is 0
                return yhigh*lowerP;
            }

            var count = twoInfo.TwoGramCount;

            var first = Math.Max(0.0, count - D2(count))/denom;
            var pokemon = first + yhigh*lowerP;
            return pokemon;
        }

        private double GetLowerP(params string[] w)
        {
            int oneIndex;
            if (!ReverseWordList.TryGetValue(w[0], out oneIndex))
            {
                //with no nwStar, ylow = 0
                //if wStar doesn't exist, ww doesn't exist, which leads to Starww doesn't exist, and neither do any terms in first
                return 0.0;
            }

            var oneInfo = DataSet[oneIndex];
            var nwStar = oneInfo.NwStarCount;
            var lowerP = GetLowerLowerP(w[1]);
            var ylow = _d21*nwStar.N1WStar + _d22*nwStar.N2WStar + _d23*nwStar.N3PlusWStar;

            TwoStringInfo twoInfo;
            int twoIndex;
            if (!ReverseWordList.TryGetValue(w[1], out twoIndex) || !oneInfo.TwoGrams.TryGetValue(twoIndex, out twoInfo) || twoInfo.N1PlusStarww == 0)
            {
                //last ditch attempt to have some kind of lower bound estimate
                if (oneInfo.N1PlusStarwStar != 0)
                {
                    return lowerP*ylow/oneInfo.N1PlusStarwStar;
                }

                return 0.0;
            }

            //_ww means ww and _w_ exist, so no need to TryGet
            var n1PlusStarwStar = oneInfo.N1PlusStarwStar;

            ylow = ylow/n1PlusStarwStar;
            var first = Math.Max(0.0, twoInfo.N1PlusStarww - D2(twoInfo.TwoGramCount))/n1PlusStarwStar;
            var pokemon = first + ylow*lowerP;
            return pokemon;
        }

        private double GetLowerLowerP(string w)
        {
            int oneIndex;
            if (!ReverseWordList.TryGetValue(w, out oneIndex))
            {
                return 0.0;
            }

            var oneInfo = DataSet[oneIndex];
            return oneInfo.N1PlusStarw/TwoGramCount;
        }

        #endregion

        #region D

        private void SetDs()
        {
            var d = N11/(double) (N11 + 2*N12);
            _d11 = 1 - 2*d*N12/N11;
            _d12 = 2 - 3*d*N13/N12;
            _d13 = 3 - 4*d*N14/N13;

            var d2 = N21/(double) (N21 + 2*N22);
            _d21 = 1 - 2*d2*N22/N21;
            _d22 = 2 - 3*d*N23/N22;
            _d23 = 3 - 4*d*N24/N23;
        }

        private double D(uint count)
        {
            switch (count)
            {
                case 0:
                    return 0;
                case 1:
                    return _d11;
                case 2:
                    return _d12;
                default:
                    return _d13;
            }
        }

        private double D2(uint count)
        {
            switch (count)
            {
                case 0:
                    return 0;
                case 1:
                    return _d21;
                case 2:
                    return _d22;
                default:
                    return _d23;
            }
        }

        #endregion
    }
}