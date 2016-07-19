using Augury.Lucene;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Augury.Test.Serialization
{
    [TestClass]
    public class SpellCheckSerializationTest : SerializationTestBase<SpellCheck>
    {
        protected override SpellCheck CreateInstance()
        {
            const string files = @"..\..\SampleTexts\The lost world.txt";
            return new SpellCheck(CorpusProcessor.ParseWords(files), new JaroWinkler());
        }
    }
}
