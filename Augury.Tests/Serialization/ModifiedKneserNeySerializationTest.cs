using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Augury.Test.Serialization
{
    [TestClass]
    public class ModifiedKneserNeySerializationTest : SerializationTestBase<ModifiedKneserNey>
    {
        protected override ModifiedKneserNey CreateInstance()
        {
            const string files = @"..\..\SampleTexts\The lost world.txt";
            return CorpusProcessor.CreateModifiedKneserNey(files);
        }
    }
}
