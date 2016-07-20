using Augury.Base;
using Augury.Lucene;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Augury.Test.StringMetric
{
    [TestClass]
    public class NGramDistanceTest : StringMetricTestBase
    {
        protected override IStringMetric StringMetric => new NGramDistance();
    }
}
