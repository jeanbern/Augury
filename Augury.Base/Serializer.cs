using System.IO;

namespace Augury.Base
{
    public abstract class Serializer<T> : SerializationBase
    {
        public abstract T Deserialize(Stream stream);

        public abstract void Serialize(Stream stream, T data);
    }
}
