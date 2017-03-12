using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Augury.Test.DAWG
{
    [TestClass]
    public class DawgBuilderTest : DawgTestBase<TestDawgNode>
    {
        protected override TestDawgNode CreateInstance(IEnumerable<string> words)
        {
            var builder = new DawgBuilder();
            foreach (var word in words)
            {
                builder.Insert(word);
            }

            var root = builder.Finish();
            return new TestDawgNode(root);
        }


        [TestMethod]
        public void FrequentWordList()
        {
            var wordString = File.ReadAllText(@"..\..\FrequentWordList.txt");
            var words = new HashSet<string>();
            foreach (var sentence in Tokenizer.GetSentences(wordString))
            {
                foreach (var word in sentence)
                {
                    words.Add(word.ToLowerInvariant());
                }
            }

            var wordArray = words.ToArray();
            Array.Sort(wordArray, StringComparer.OrdinalIgnoreCase);
            var dawg = CreateInstance(wordArray);
            var allWords = dawg.AllWords();
            Assert.IsTrue(allWords.Contains("love"));
            Assert.IsTrue(allWords.Contains("judgment"));
            Assert.IsTrue(allWords.Contains("economically"));
            Assert.IsTrue(allWords.Contains("engineering"));
        }
    }
}
