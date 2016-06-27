using Augury.Comparers;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Augury
{
    public static class CorpusProcessor
    {
        public static Auger Create(bool qikit, List<string> words, params string[] filenames)
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

            return new Auger(d3, d2, d1, words)
            {
                English = !qikit
            };
        }
    }
}
