using Augury.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Augury
{
    internal class AugerSerializer : Serializer<Auger>
    {
        public override Auger Deserialize(Stream stream)
        {
            var dataSet = DeserializeListOneStringInfo(ReadChunk(stream, "dataSet"));
            var reverseWordList = new OrderedDictionarySerializer().Deserialize(stream);
            var symPred = new SpellCheckSerializer().Deserialize(stream);
            var nValues = ReadChunk(stream, "nValues");

            return new Auger(dataSet, symPred, reverseWordList,
                        BitConverter.ToUInt32(nValues, 0),
                        BitConverter.ToUInt32(nValues, 4),
                        BitConverter.ToUInt32(nValues, 8),
                        BitConverter.ToUInt32(nValues, 12),
                        BitConverter.ToUInt32(nValues, 16),
                        BitConverter.ToUInt32(nValues, 20),
                        BitConverter.ToUInt32(nValues, 24),
                        BitConverter.ToUInt32(nValues, 28),
                        BitConverter.ToDouble(nValues, 32))
            {
                English = BitConverter.ToBoolean(nValues, 40)
            };
        }

        public override void Serialize(Stream stream, Auger data)
        {
            var bytes = Encapsulate(Serialize(data.DataSet));
            stream.Write(bytes, 0, bytes.Length);

            new OrderedDictionarySerializer().Serialize(stream, data.ReverseWordList);

            new SpellCheckSerializer().Serialize(stream, data.SpellChecker);

            stream.Write(BitConverter.GetBytes(41), 0, 4);
            bytes = BitConverter.GetBytes(data.N11);
            stream.Write(bytes, 0, bytes.Length);
            bytes = BitConverter.GetBytes(data.N12);
            stream.Write(bytes, 0, bytes.Length);
            bytes = BitConverter.GetBytes(data.N13);
            stream.Write(bytes, 0, bytes.Length);
            bytes = BitConverter.GetBytes(data.N14);
            stream.Write(bytes, 0, bytes.Length);
            bytes = BitConverter.GetBytes(data.N21);
            stream.Write(bytes, 0, bytes.Length);
            bytes = BitConverter.GetBytes(data.N22);
            stream.Write(bytes, 0, bytes.Length);
            bytes = BitConverter.GetBytes(data.N23);
            stream.Write(bytes, 0, bytes.Length);
            bytes = BitConverter.GetBytes(data.N24);
            stream.Write(bytes, 0, bytes.Length);
            bytes = BitConverter.GetBytes(data.TwoGramCount);
            stream.Write(bytes, 0, bytes.Length);
            bytes = BitConverter.GetBytes(data.English);
            stream.Write(bytes, 0, bytes.Length);
        }


        protected static byte[] Serialize(List<OneStringInfo> data)
        {
            return Concat(data.Select(Serialize));
        }

        protected static List<OneStringInfo> DeserializeListOneStringInfo(byte[] bytes)
        {
            var list = new List<OneStringInfo>();
            var index = 0;
            var maxLen = bytes.Length;
            while (index < maxLen)
            {
                var nextLength = BitConverter.ToInt32(bytes, index);
                index += 4;
                var value = DeserializeOneStringInfo(bytes, index, nextLength);
                index += nextLength;
                list.Add(value);
            }

            return list;
        }

        protected static byte[] Serialize(KeyValuePair<int, TwoStringInfo> data)
        {
            return Encapsulate(BitConverter.GetBytes(data.Key), Serialize(data.Value));
        }

        protected static KeyValuePair<int, TwoStringInfo> DeserializeKvpIntTwoStringInfo(byte[] bytes, int start, int length)
        {
            var key = BitConverter.ToInt32(bytes, start);
            var twoStringInfo = DeserializeTwoStringInfo(bytes, start + 4, length - 4);
            return new KeyValuePair<int, TwoStringInfo>(key, twoStringInfo);
        }

        protected static byte[] Serialize(Dictionary<int, TwoStringInfo> data)
        {
            return Concat(data.Select(Serialize));
        }

        protected static Dictionary<int, TwoStringInfo> DeserializeDictionaryIntTwoStringInfo(byte[] bytes, int start = 0, int length = -1)
        {
            var dictionary = new Dictionary<int, TwoStringInfo>();
            var index = 0;
            var maxLen = length < 0 ? bytes.Length : length;
            while (index < maxLen)
            {
                var nextLength = BitConverter.ToInt32(bytes, start + index);
                index += 4;
                var nextPair = DeserializeKvpIntTwoStringInfo(bytes, start + index, nextLength);
                index += nextLength;
                dictionary.Add(nextPair.Key, nextPair.Value);
            }

            return dictionary;
        }

        protected static byte[] Serialize(OneStringInfo data)
        {
            return Encapsulate(BitConverter.GetBytes(data.OneGramCount),
                BitConverter.GetBytes(data.N1PlusStarwStar),
                BitConverter.GetBytes(data.N1PlusStarw),
                Serialize(data.NwStarCount),
                Encapsulate(Serialize(data.MostLikelies)),
                Encapsulate(Serialize(data.TwoGrams)));
        }

        protected static OneStringInfo DeserializeOneStringInfo(byte[] bytes, int start, int length)
        {
            var oneGramCount = BitConverter.ToUInt32(bytes, start);
            var n1PlusStarwStar = BitConverter.ToUInt32(bytes, start + 4);
            var n1PlusStarw = BitConverter.ToUInt32(bytes, start + 8);
            var nwStar = DeserializeNwStarCount(bytes, start + 12);
            var mostLikeliesLength = BitConverter.ToInt32(bytes, start + 24);
            var mostLikelies = DeserializeListInt(bytes, start + 28, mostLikeliesLength);
            var twoGramsLength = BitConverter.ToInt32(bytes, start + mostLikeliesLength + 28);
            var twoGrams = DeserializeDictionaryIntTwoStringInfo(bytes, start + mostLikeliesLength + 32, twoGramsLength);

            return new OneStringInfo
            {
                MostLikelies = mostLikelies,
                N1PlusStarw = n1PlusStarw,
                N1PlusStarwStar = n1PlusStarwStar,
                NwStarCount = nwStar,
                OneGramCount = oneGramCount,
                TwoGrams = twoGrams
            };
        }

        protected static byte[] Serialize(TwoStringInfo data)
        {
            return Concat(BitConverter.GetBytes(data.TwoGramCount),
                BitConverter.GetBytes(data.N1PlusStarww),
                Serialize(data.NwwStarCount),
                Encapsulate(Serialize(data.MostLikelies)),
                Encapsulate(Serialize(data.ThreeGramCounts)));
        }

        protected static TwoStringInfo DeserializeTwoStringInfo(byte[] bytes, int start, int length)
        {
            var twoGramCount = BitConverter.ToUInt32(bytes, start);
            var n1PlusStarww = BitConverter.ToUInt32(bytes, start + 4);
            var nwwStar = DeserializeNwStarCount(bytes, start + 8);
            var mostLikeliesLength = BitConverter.ToInt32(bytes, start + 20);
            var mostLikelies = DeserializeListInt(bytes, start + 24, mostLikeliesLength);
            var threeGramsLength = BitConverter.ToInt32(bytes, start + mostLikeliesLength + 24);
            var threeGrams = DeserializeDictionaryIntUint(bytes, start + mostLikeliesLength + 28, threeGramsLength);

            return new TwoStringInfo
            {
                MostLikelies = mostLikelies,
                N1PlusStarww = n1PlusStarww,
                NwwStarCount = nwwStar,
                TwoGramCount = twoGramCount,
                ThreeGramCounts = threeGrams
            };
        }

        protected static byte[] Serialize(NwStarCount data)
        {
            return Concat(BitConverter.GetBytes(data.N1WStar), BitConverter.GetBytes(data.N2WStar), BitConverter.GetBytes(data.N3PlusWStar));
        }

        protected static NwStarCount DeserializeNwStarCount(byte[] bytes, int start)
        {
            return new NwStarCount
            {
                N1WStar = BitConverter.ToUInt32(bytes, start),
                N2WStar = BitConverter.ToUInt32(bytes, start + 4),
                N3PlusWStar = BitConverter.ToUInt32(bytes, start + 8)
            };
        }
    }
}
