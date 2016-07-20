using Augury.Base;
using Augury.Lucene;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Augury.Test.StringMetric
{
    [TestClass]
    public class LevenshteinDistanceTest : StringMetricTestBase
    {
        protected override IStringMetric StringMetric => new LevensteinDistance();
    }
}
