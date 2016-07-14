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
        public static Auger Create(params string[] filenames)
        {
            var d1 = new Dictionary<string, uint>();
            var d2 = new Dictionary<string[], uint>(new StringArrayEqualityComparerTwo());
            var d3 = new Dictionary<string[], uint>(new StringArrayEqualityComparerThree());

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

            var mkn = new ModifiedKnesserNey(d3, d2, d1);
            var words = d1.Keys.ToArray();
            Array.Sort(words, StringComparer.OrdinalIgnoreCase);
            var sc = new SpellCheck(words, new BoundedJaroWinkler());
            return new Auger(sc, mkn, mkn);
        }

        public static Auger Create(List<string> words, params string[] filenames)
        {
            var d1 = new Dictionary<string, uint>();
            var d2 = new Dictionary<string[], uint>(new StringArrayEqualityComparerTwo());
            var d3 = new Dictionary<string[], uint>(new StringArrayEqualityComparerThree());

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

            var mkn = new ModifiedKnesserNey(d3, d2, d1);
            var sc = new SpellCheck(words, new BoundedJaroWinkler());
            return new Auger(sc, mkn, mkn);
        }
    }
}
