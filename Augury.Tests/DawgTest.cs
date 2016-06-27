using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Augury.Test
{
    [TestClass]
    public class DawgTest
    {
        [TestMethod]
        public void SmallCorrectnessTest()
        {
            var firstList = new List<string> { "cities", "city", "pities", "pity" };
            var dawg = new TestDawg(firstList);
            var nodes = dawg.AllWords();
            Assert.AreEqual(7, dawg.NodeCount);
            Assert.AreEqual(8, dawg.EdgeCount);
            Assert.IsTrue(nodes.SequenceEqual(firstList));

            var secondList = new List<string> { "cities", "city", "pities", "pitiful", "pity", "pretty" };
            dawg = new TestDawg(secondList);
            nodes = dawg.AllWords();
            Assert.AreEqual(17, dawg.NodeCount);
            Assert.AreEqual(21, dawg.EdgeCount);
            Assert.IsTrue(nodes.SequenceEqual(secondList));
        }
    }
}
