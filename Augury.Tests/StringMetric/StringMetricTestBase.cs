using Augury.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Augury.Test.StringMetric
{
    [TestClass]
    public abstract class StringMetricTestBase
    {
        protected virtual int MaxComparableStringLength => 50;

        protected abstract IStringMetric StringMetric { get; }

        [TestMethod]
        public void SameStringTest()
        {
            var stringsToTest = new List<string>
            {
                null,
                string.Empty,
                new string(' ', 1),
                new string('a', 1),
                new string('ε', 1),
                new string(' ', 10),
                new string('a', 10),
                new string('ε', 10),
                new string(' ', MaxComparableStringLength),
                new string('a', MaxComparableStringLength),
                new string('ε', MaxComparableStringLength),
            };

            foreach (var value in stringsToTest)
            { 
                var similarity = StringMetric.Similarity(value, value);
                Assert.AreEqual(1, similarity);
            }
        }

        [TestMethod]
        public void TranspositionTest()
        {
            var value = "ab";
            var transposition = "ba";
            var similarity = StringMetric.Similarity(value, transposition);
            Assert.IsTrue(similarity >= 0);
            Assert.IsTrue(similarity <= 1);

            value = "abcd";
            transposition = "acbd";
            var worseTransposition = "adbc";
            similarity = StringMetric.Similarity(value, transposition);
            var worseSimilarity = StringMetric.Similarity(value, worseTransposition);
            Assert.IsTrue(similarity >= worseSimilarity);

            worseTransposition = "badc";
            similarity = StringMetric.Similarity(value, transposition);
            worseSimilarity = StringMetric.Similarity(value, worseTransposition);
            Assert.IsTrue(similarity >= worseSimilarity);
        }

        [TestMethod]
        public void ReplacementTest()
        {
            var value = "abcd";
            var replacement = "axcd";
            var similarity = StringMetric.Similarity(value, replacement);
            Assert.IsTrue(similarity >= 0);
            Assert.IsTrue(similarity <= 1);

            value = "abcd";
            replacement = "abxd";
            var worseTransposition = "axcd";
            similarity = StringMetric.Similarity(value, replacement);
            var worseReplacement = StringMetric.Similarity(value, worseTransposition);
            Assert.IsTrue(similarity >= worseReplacement);

            worseTransposition = "axyd";
            similarity = StringMetric.Similarity(value, replacement);
            worseReplacement = StringMetric.Similarity(value, worseTransposition);
            Assert.IsTrue(similarity >= worseReplacement);
        }

        [TestMethod]
        public void InsertionTest()
        {
            var value = "abcd";
            var insertion = "axbcd";
            var similarity = StringMetric.Similarity(value, insertion);
            Assert.IsTrue(similarity >= 0);
            Assert.IsTrue(similarity <= 1);

            value = "abcd";
            insertion = "abcxd";
            var worseTransposition = "axbcd";
            similarity = StringMetric.Similarity(value, insertion);
            var worseInsertion = StringMetric.Similarity(value, worseTransposition);
            Assert.IsTrue(similarity >= worseInsertion);

            worseTransposition = "axbcyd";
            similarity = StringMetric.Similarity(value, insertion);
            worseInsertion = StringMetric.Similarity(value, worseTransposition);
            Assert.IsTrue(similarity >= worseInsertion);
        }
    }
}
