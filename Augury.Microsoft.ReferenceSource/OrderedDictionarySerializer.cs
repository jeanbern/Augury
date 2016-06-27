using Augury.Base;
using System.IO;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    public class OrderedDictionarySerializer : Serializer<OrderedDictionary<string, int>>
    {

        public override OrderedDictionary<string, int> Deserialize(Stream stream)
        {
            return DeserializeDictionaryStringint(ReadChunk(stream, "dataSet"));
        }

        public override void Serialize(Stream stream, OrderedDictionary<string, int> data)
        {
            var bytes = Encapsulate(Serialize(data));
            stream.Write(bytes, 0, bytes.Length);
        }
        protected static byte[] Serialize(OrderedDictionary<string, int> data)
        {
            return Concat(data.Select(Serialize));
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
                var nextPair = DeserializeKvpStringint(bytes, index, nextLength);
                dict.Add(nextPair.Key, nextPair.Value);
                index += nextLength + 4;
            }

            return dict;
        }
    }
}
