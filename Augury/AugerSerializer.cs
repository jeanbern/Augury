using Augury.Base;
using System.IO;

namespace Augury
{
    internal class AugerSerializer : ISerializer<Auger>
    {
        public Auger Deserialize(Stream stream)
        {
            var spellChecker = Serialization.DeserializeInterface<IPrefixLookup>(stream);
            var lm = Serialization.DeserializeInterface<ILanguageModel>(stream);
            var nwm = lm as INextWordModel ?? Serialization.DeserializeInterface<INextWordModel>(stream);

            return new Auger(spellChecker, lm, nwm);
        }
        
        public void Serialize(Stream stream, Auger data)
        {
            Serialization.SerializeInterface(stream, data.SpellChecker);
            Serialization.SerializeInterface(stream, data.LanguageModel);

            if (data.LanguageModel is INextWordModel)
            {
                return;
            }

            Serialization.SerializeInterface(stream, data.NextWordModel);
        }
    }
}
