using System.Diagnostics;
using System.IO;

namespace Augury.Test
{
    internal static class AugerHelper
    {
        public static Auger LoadOrCreateCompleteCorpus()
        {
            Auger auger;
            if (File.Exists("Auger.aug"))
            {
                using (var stream = new FileStream("Auger.aug", FileMode.OpenOrCreate))
                {
                    Debug.WriteLine("Loading serialized Auger");
                    auger = Auger.Load(stream);
                }
            }
            else
            {
                Debug.WriteLine("Creating new auger from corpus.");
                auger = CorpusProcessor.Create(Directory.GetFiles(@"..\..\SampleTexts\", "*.txt"));
                using (var stream = new FileStream("Auger.aug", FileMode.OpenOrCreate))
                {
                    auger.Save(stream);
                }
            }

            return auger;
        }
    }
}
