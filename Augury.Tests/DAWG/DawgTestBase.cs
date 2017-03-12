using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Augury.Test.DAWG
{
    [TestClass]
    public abstract class DawgTestBase<T> where T : ITestDawg
    {
        protected abstract T CreateInstance(IEnumerable<string> words);

        [TestMethod]
        public void SmallWordTest()
        {
            var firstList = new List<string> { "cities", "city", "pities", "pity" };
            var dawg = CreateInstance(firstList);
            Assert.AreEqual(7, dawg.NodeCount);

            dawg = CreateInstance(firstList);
            Assert.AreEqual(8, dawg.EdgeCount);

            dawg = CreateInstance(firstList);
            var nodes = dawg.AllWords();
            Assert.IsTrue(nodes.SequenceEqual(firstList));
            
            var secondList = new List<string> { "cities", "city", "pities", "pitiful", "pity", "pretty" };
            dawg = CreateInstance(secondList);
            Assert.AreEqual(17, dawg.NodeCount);

            dawg = CreateInstance(secondList);
            Assert.AreEqual(21, dawg.EdgeCount);

            dawg = CreateInstance(secondList);
            nodes = dawg.AllWords();
            Assert.IsTrue(nodes.SequenceEqual(secondList));
        }

        [TestMethod]
        public void OddWordTest()
        {
            var words = new List<string> { "one", "two", "hillbilly", "hill", "hil", "billy", "goat", "chelsea" };
            var wordArray = words.ToArray();
            Array.Sort(wordArray, StringComparer.OrdinalIgnoreCase);
            var dawg = CreateInstance(wordArray);
            var allWords = dawg.AllWords();

            Assert.AreEqual(words.Count, allWords.Count);
            Assert.IsTrue(allWords.SequenceEqual(wordArray));
        }

        [TestMethod]
        public void LargeWordTest()
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

            Assert.AreEqual(wordArray.Length, allWords.Count);

            /*
            var strings = new[]
            {
                "arguement",
                "employeer",
                "engineing",
                "entitly",
                "indicor",
                "investigor",
                "judgement",
                "liberalty",
                "malel",
                "manufacturering",
                "plannering",
                "runnering",
                "tilel",
                "versious",
                "watcher"
            };
            Assert.IsTrue(allWords.Except(wordArray).SequenceEqual(strings));*/

            /*
            var strings2 = new[]
            {
                "argument",
                "employer",
                "engineering",
                "entity",
                "indicator",
                "investigator",
                "judgment",
                "liberty",
                "mall",
                "manufacturing",
                "planning",
                "running",
                "till",
                "versus",
                "water"
            };
            Assert.IsTrue(wordArray.Except(allWords).SequenceEqual(strings2));*/

            Assert.IsTrue(allWords.SequenceEqual(wordArray));
        }

        [TestMethod]
        public void ReproductionCase()
        {
            var words = new[]
            {
                "argue",
                "argument",

                "employee",
                "eploy",
                "employer",
                "employment",
                "unemployment",

                "engine",
                "engineer",
                "engineering",

                "identity",
                "entitle",
                "entity",

                "indicate",
                "indication",
                "indicator",

                "investigate",
                "investigation",
                "investigator",

                "judge",
                "judgment",

                "liberal",
                "liberty",
                "deliberately",

                "small",
                "animal",
                "normal",
                "female",
                "male",
                "formal",
                "normally",
                "mall",
                "minimal",
                "informal",

                "manufacturer",
                "manufacturing",

                "plan",
                "plant",
                "plane",
                "planet",
                "explanation",
                "planning",
                "airplane",
                "planner",

                "running",
                "runner",

                "still",
                "till",

                "version",
                "conversation",
                "university",
                "universe",
                "diversity",
                "universal",
                "diverse",
                "controversy",
                "controversial",
                "versus",
                "anniversary",
                "reverse",
                "oversee",
                "conversion",

                "water",
                "watch"
            };
            
            var wordArray = words.ToArray();
            Array.Sort(wordArray, StringComparer.InvariantCultureIgnoreCase);
            var dawg = CreateInstance(wordArray);
            var allWords = dawg.AllWords();

            Assert.AreEqual(words.Length, allWords.Count);
            Assert.IsTrue(allWords.SequenceEqual(wordArray));
        }

        [TestMethod]
        public void HugeWordTest()
        {
            var filenames = Directory.GetFiles(@"..\..\SampleTexts", "*.txt");
            var readFiles = filenames.Select(File.ReadAllText);
            var files = readFiles.Select(Tokenizer.GetSentences);
            var words = new HashSet<string>();
            foreach (var sentences in files)
            {
                foreach (var sentence in sentences)
                {
                    var loweredSentence = sentence.Select(Tokenizer.ToLowercase);
                    foreach (var word in loweredSentence)
                    {
                        words.Add(word);
                    }
                }
            }

            var wordArray = words.ToArray();
            Array.Sort(wordArray, StringComparer.OrdinalIgnoreCase);
            var dawg = CreateInstance(wordArray);

            var allWords = dawg.AllWords();
            Assert.AreEqual(wordArray.Length, allWords.Count);

            Debug.WriteLine(string.Join(", ", allWords.Except(wordArray)));
            Debug.WriteLine(string.Join(", ", words.Except(allWords)));

            Assert.IsTrue(allWords.SequenceEqual(wordArray));
        }
    }
}
