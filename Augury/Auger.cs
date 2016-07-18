using Augury.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Augury
{
    public class Auger
    {
        public IPrefixLookup SpellChecker { get; }
        public ILanguageModel LanguageModel { get; }
        public INextWordModel NextWordModel { get; }

        public Auger(IPrefixLookup spellchecker, ILanguageModel languageModel, INextWordModel nextWordModel)
        {
            SpellChecker = spellchecker;
            LanguageModel = languageModel;
            NextWordModel = nextWordModel;
        }

        //The larger this value, the more words we check for history probability.
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
        /// </remarks>
        public IReadOnlyList<string> Predict(IReadOnlyList<string> history)
        {
            var lastIndex = history.Count - 1;
            if (lastIndex < 0)
            {
                return new List<string>();
            }

            var lastWord = history[lastIndex];
            history = history.Select(x => x.ToLowerInvariant()).ToList();

            var partial = history[lastIndex];
            //Only care about the last 2, since we deal with 3-grams
            var previousCount = Math.Max(lastIndex, 2);
            var previous = history.Skip(history.Count - previousCount).ToArray();
            
            if (string.IsNullOrWhiteSpace(partial))
            {
                return NextWordModel.NextWord(previous);
            }

            //Words obtained via correcting and/or starting with partial.
            //Does not care about the word history.
            var spellchecks = SpellChecker.PrefixLookup(partial, MaxSpellCheckResults);

            var possibleWordValue = new string[previousCount + 1];
            Array.Copy(previous, possibleWordValue, previousCount);

            var possibleSpellchecks = new Dictionary<string, double>();
            //Now we take our possible spell-checked words and see if any of them fit well with the history.
            foreach (var spellcheckPair in spellchecks)
            {
                possibleWordValue[previousCount] = spellcheckPair.Word;
                var value = LanguageModel.Evaluate(possibleWordValue);

                possibleSpellchecks.Add(spellcheckPair.Word, value > 0 ? value*spellcheckPair.Similarity : spellcheckPair.Similarity - 1);
            }
            
            return possibleSpellchecks
                .OrderByDescending(x => x.Value)
                .Select(x => x.Key)
                .Take(MaxResults)
                .Select(x => Tokenizer.CapitalizeFromTemplate(lastWord, x, lastIndex == 0))
                .ToList();
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

        public override bool Equals(object obj)
        {
            var other = obj as Auger;
            return other != null && Equals(other);
        }

        protected bool Equals(Auger other)
        {
            return Equals(SpellChecker, other.SpellChecker) &&
                   Equals(LanguageModel, other.LanguageModel) &&
                   Equals(NextWordModel, other.NextWordModel);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = SpellChecker?.GetHashCode() ?? 0;
                hashCode = (hashCode*397) ^ (LanguageModel?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (NextWordModel?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}