using Augury.Base;
using System.IO;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    public class OrderedDictionarySerializer : ISerializer<OrderedDictionary<string, int>>
    {

        public OrderedDictionary<string, int> Deserialize(Stream stream)
        {
            return DeserializeDictionaryStringint(Serialization.ReadChunk(stream, "dataSet"));
        }

        public void Serialize(Stream stream, OrderedDictionary<string, int> data)
        {
            var bytes = Serialization.Encapsulate(Serialize(data));
            stream.Write(bytes, 0, bytes.Length);
        }
        protected static byte[] Serialize(OrderedDictionary<string, int> data)
        {
            return Serialization.Concat(data.Select(Serialization.Serialize));
        }

        protected static OrderedDictionary<string, int> DeserializeDictionaryStringint(byte[] bytes)
        {
            var dict = new OrderedDictionary<string, int>();
            var index = 0;
            var maxLen = bytes.Length;
            while (index < maxLen)
            {
                var nextLength = BitConverter.ToInt32(bytes, index);
                index += 4;
                var nextPair = Serialization.DeserializeKvpStringint(bytes, index, nextLength);
                dict.Add(nextPair.Key, nextPair.Value);
                index += nextLength + 4;
            }

            return dict;
        }
    }
}
