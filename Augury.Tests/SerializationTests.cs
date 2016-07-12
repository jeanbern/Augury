using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Augury.Test
{
    [TestClass]
    public class SerializationTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            const string files = @"..\..\SampleTexts\The lost world.txt";

            var auger = CorpusProcessor.Create(files);

            using (var writer = new StreamWriter("test.aug"))
            {
                auger.Save(writer.BaseStream);
            }

            Auger v2;
            using (var reader = new StreamReader("test.aug"))
            {
                v2 = Auger.Load(reader.BaseStream);
            }

            //Console.WriteLine(v2.Predict(words));
            Console.WriteLine("success");
        }
    }
}
