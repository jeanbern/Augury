using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Augury.Test.Serialization
{
    [TestClass]
    public class ModifiedKnesserNeySerializationTest : SerializationTestBase<ModifiedKnesserNey>
    {
        protected override ModifiedKnesserNey CreateInstance()
        {
            const string files = @"..\..\SampleTexts\The lost world.txt";
            return CorpusProcessor.CreateModifiedKnesserNey(files);
        }
    }
}
