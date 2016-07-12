using Augury.Base;
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
        public IPrefixLookup SpellChecker { get; protected set; }
        public ILanguageModel LanguageModel { get; protected set; }
        public INextWordModel NextWordModel { get; protected set; }

        public Auger(IPrefixLookup spellchecker, ILanguageModel languageModel, INextWordModel nextWordModel)
        {
            SpellChecker = spellchecker;
            LanguageModel = languageModel;
            NextWordModel = nextWordModel;
        }

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
                return NextWordModel.NextWord(previous);
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
                var value = LanguageModel.Evaluate(newList.ToArray());

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
    }
}