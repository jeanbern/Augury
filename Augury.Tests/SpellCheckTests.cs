using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Augury.Test
{
    [TestClass]
    public class SpellCheckTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var auger = AugerHelper.LoadOrCreateCompleteCorpus();
            var results = auger.SpellChecker.PrefixLookup("love", 100).Select(x => x.Word).ToList();
            Assert.IsTrue(results.Contains("love"));
            Assert.IsTrue(results.Contains("loved"));
            Assert.IsTrue(results.Contains("live"));

            var allWords = ((Dawg) auger.SpellChecker).AllWords();
            foreach (var word in results)
            {
                Assert.IsTrue(allWords.Contains(word));
            }
        }
    }
}
