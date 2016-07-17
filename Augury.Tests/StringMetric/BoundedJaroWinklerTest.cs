using Augury.Base;
using Augury.Lucene;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Augury.Test.StringMetric
{
    [TestClass]
    public class BoundedJaroWinklerTest : JaroWinklerTest
    {
        protected override IStringMetric StringMetric => new BoundedJaroWinkler();

        [TestMethod]
        public void BoundedIsLowerThanNonBounded()
        {
            var unboundedMetric = new JaroWinkler();
            
            var originalStrings = new []
            {
                "ab",
                "abcd",
                "abcd",
                "abcd",

                "abcd",
                "abcd",
                "abcd",

                "abcd",
                "abcd",
                "abcd",
                "abcd"
            };

            var modifiedStrings = new[]
            {
                "ab",
                "acbd",
                "adbc",
                "badc",

                "axcd",
                "abxd",
                "axyd",

                "axbcd",
                "abcxd",
                "axbcd",
                "axbcyd"
            };

            Assert.AreEqual(originalStrings.Length, modifiedStrings.Length);

            for (var i = 0; i < originalStrings.Length; i++)
            {
                var original = originalStrings[i];
                var modified = modifiedStrings[i];

                var similarity = unboundedMetric.Similarity(original, modified);
                var boundedSimilarity = StringMetric.Similarity(original, modified);
                Assert.IsTrue(boundedSimilarity <= similarity);
            }
        }
    }
}
