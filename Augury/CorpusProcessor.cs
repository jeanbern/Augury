using Augury.Comparers;
using Augury.Lucene;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Augury
{
    public static class CorpusProcessor
    {
        public static IEnumerable<string> ParseWords(params string[] filenames)
        {
            var readFiles = filenames.Select(File.ReadAllText);
            var files = readFiles.Select(Tokenizer.GetSentences);
            var results = new HashSet<string>();

            foreach (var sentences in files)
            {
                foreach (var sentence in sentences)
                {
                    foreach (var word in sentence)
                    {
                        results.Add(Tokenizer.ToLowercase(word));
                    }
                }
            }

            var resultArray = results.ToArray();
            Array.Sort(resultArray, StringComparer.OrdinalIgnoreCase);
            return resultArray;
        }

        public static Auger Create(params string[] filenames)
        {
            var d1 = new Dictionary<string, uint>();
            var d2 = new Dictionary<string[], uint>(new StringArrayEqualityComparerTwo());
            var d3 = new Dictionary<string[], uint>(new StringArrayEqualityComparerThree());
            
            Read(ref d1, ref d2, ref d3, filenames);

            var mkn = new ModifiedKneserNey(d3, d2, d1);
            var words = d1.Keys.ToArray();
            Array.Sort(words, StringComparer.OrdinalIgnoreCase);
            var sc = new SpellCheck(words, new BoundedJaroWinkler());
            return new Auger(sc, mkn, mkn);
        }

        public static ModifiedKneserNey CreateModifiedKneserNey(params string[] filenames)
        {
            var d1 = new Dictionary<string, uint>();
            var d2 = new Dictionary<string[], uint>(new StringArrayEqualityComparerTwo());
            var d3 = new Dictionary<string[], uint>(new StringArrayEqualityComparerThree());

            Read(ref d1, ref d2, ref d3, filenames);

            return new ModifiedKneserNey(d3, d2, d1);
        }

        internal static void Read(ref Dictionary<string, uint> d1, ref Dictionary<string[], uint> d2, ref Dictionary<string[], uint> d3, params string[] filenames)
        {
            var readFiles = filenames.Select(File.ReadAllText);
            var files = readFiles.Select(Tokenizer.GetSentences);

            foreach (var sentences in files)
            {
                foreach (var sentence in sentences)
                {
                    var loweredSentence = sentence.Select(Tokenizer.ToLowercase);
                    Tokenizer.ForwardTokenizeTogether(loweredSentence, d1, d2, d3);
                }
            }
        }
    }
}
