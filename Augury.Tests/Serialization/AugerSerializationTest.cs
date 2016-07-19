using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Augury.Test.Serialization
{
    [TestClass]
    public class AugerSerializationTest : SerializationTestBase<Auger>
    {
        protected override Auger CreateInstance()
        {
            const string files = @"..\..\SampleTexts\The lost world.txt";
            return CorpusProcessor.Create(files);
        }
    }
}
