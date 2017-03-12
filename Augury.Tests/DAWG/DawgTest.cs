using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Augury.Test.DAWG
{
    [TestClass]
    public class DawgTest : DawgTestBase<TestDawg>
    {
        protected override TestDawg CreateInstance(IEnumerable<string> words)
        {
            return new TestDawg(words);
        }
    }
}