using System;
using Augury.Base;
using Augury.Lucene;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Augury.Test.StringMetric
{
    [TestClass]
    public class JaroWinklerTest : StringMetricTestBase
    {
        protected override IStringMetric StringMetric => new JaroWinkler();
        
        [TestMethod]
        public void ExpectedValuesTest()
        {
            var value = "martha";
            var transposition = "marhta";
            var similarity = StringMetric.Similarity(value, transposition);
            Assert.IsTrue(Math.Abs(830/900.0 - similarity) < 0.0001);

            value = "jones";
            transposition = "johnson";
            similarity = StringMetric.Similarity(value, transposition);
            Assert.IsTrue(Math.Abs(.832 - similarity) < 0.001);
        }
    }
}
