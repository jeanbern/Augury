using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Augury.Base
{
    public abstract class SerializationBase
    {
        protected static byte[] ReadChunk(Stream stream, string member)
        {
            var bytes = new byte[4];
            stream.Read(bytes, 0, 4);
            var len = BitConverter.ToInt32(bytes, 0);
            bytes = new byte[len];
            var read = stream.Read(bytes, 0, len);
            if (read != len)
            {
                throw new EndOfStreamException("Was not able to read enough bytes for " + member);
            }

            return bytes;
        }

        protected static List<string> DeserializeListString(byte[] bytes)
        {
            var ret = new List<string>();
            var index = 0;
            var maxLen = bytes.Length;
            while (index < maxLen)
            {
                var nextLength = BitConverter.ToInt32(bytes, index);
                index += 4;
                var next = DeserializeString(bytes, index, nextLength);
                ret.Add(next);
                index += nextLength;
            }

            return ret;
        }


        protected static Dictionary<int, uint> DeserializeDictionaryIntUint(byte[] bytes, int start, int length)
        {
            var dict = new Dictionary<int, uint>();
            var index = 0;
            var maxLen = length;
            while (index < maxLen)
            {
                var key = BitConverter.ToInt32(bytes, start + index);
                index += 4;
                var value = BitConverter.ToUInt32(bytes, start + index);
                index += 4;
                dict.Add(key, value);
            }

            return dict;
        }

        protected static byte[] Serialize(Dictionary<int, uint> data)
        {
            var bytes = new byte[data.Count * 8];
            var index = 0;
            foreach (var u in data)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(u.Key), 0, bytes, index * 4, 4);
                ++index;
                Buffer.BlockCopy(BitConverter.GetBytes(u.Value), 0, bytes, index * 4, 4);
                ++index;
            }

            return bytes;
        }

        protected static byte[] Serialize(KeyValuePair<string, int> data)
        {
            return Concat(Serialize(data.Key), BitConverter.GetBytes(data.Value));
        }

        //cheating, this is actually length + 4
        protected static KeyValuePair<string, int> DeserializeKvpStringint(byte[] bytes, int start, int keyLength)
        {
            var key = string.Intern(Encoding.UTF8.GetString(bytes, start, keyLength));
            var value = BitConverter.ToInt32(bytes, start + keyLength);
            return new KeyValuePair<string, int>(key, value);
        }

        protected static byte[] Serialize(IEnumerable<string> str)
        {
            return Concat(str.Select(Serialize));
        }

        protected static string[] DeserializeStringArray(byte[] bytes, int start, int length)
        {
            var ret = new List<string>();
            var index = 0;
            while (index < length)
            {
                var nextLength = BitConverter.ToInt32(bytes, start + index);
                index += 4;
                ret.Add(string.Intern(Encoding.UTF8.GetString(bytes, start + index, nextLength)));
                index += nextLength;
            }

            return ret.ToArray();
        }

        protected static byte[] Serialize(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            return Encapsulate(bytes);
        }

        protected static string DeserializeString(byte[] bytes, int start, int length)
        {
            return string.Intern(Encoding.UTF8.GetString(bytes, start, length));
        }

        protected static byte[] Serialize(IReadOnlyList<int> data)
        {
            var ret = new byte[data.Count * 4];

            for (var x = 0; x < data.Count; x++)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(data[x]), 0, ret, 4 * x, 4);
            }

            return ret;
        }

        protected static List<int> DeserializeListInt(byte[] bytes, int start, int length)
        {
            var results = new List<int>(length / 4);
            for (var x = 0; x < results.Capacity; x++)
            {
                results.Add(BitConverter.ToInt32(bytes, start + 4 * x));
            }

            return results;
        }

        protected static int[] DeserializeArrayInt(byte[] bytes, int start, int length)
        {
            var results = new int[length / 4];
            for (var x = 0; x < results.Length; x++)
            {
                results[x] = BitConverter.ToInt32(bytes, start + 4 * x);
            }

            return results;
        }

        protected static byte[] Serialize(IReadOnlyList<ushort> data)
        {
            var ret = new byte[data.Count * 2];

            for (var x = 0; x < data.Count; x++)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(data[x]), 0, ret, 2 * x, 2);
            }

            return ret;
        }

        protected static ushort[] DeserializeArrayUShort(byte[] bytes, int start, int length)
        {
            var results = new ushort[length / 2];
            for (var x = 0; x < results.Length; x++)
            {
                results[x] = BitConverter.ToUInt16(bytes, start + 2 * x);
            }

            return results;
        }

        protected static byte[] Serialize(char[] data)
        {
            var chars = Encoding.UTF8.GetBytes(data);
            var bytes = new byte[chars.Length + 4];
            Buffer.BlockCopy(BitConverter.GetBytes(chars.Length), 0, bytes, 0, 4);
            Buffer.BlockCopy(chars, 0, bytes, 4, chars.Length);
            return bytes;
        }

        protected static char[] DeserializeArrayChar(byte[] bytes, int start, int length)
        {
            var results = new char[Encoding.UTF8.GetCharCount(bytes, start, length)];
            Encoding.UTF8.GetChars(bytes, start, length, results, 0);
            return results;
        }
        //Assumes the 2nd dimension is already encapsulated, or does not need to be
        protected static byte[] Encapsulate(params byte[][] bytes)
        {
            var size = bytes.Aggregate(0, (intt, bytee) => intt + bytee.Length);
            var ret = new byte[size + 4];
            Buffer.BlockCopy(BitConverter.GetBytes(size), 0, ret, 0, 4);
            var index = 4;
            foreach (var bytee in bytes)
            {
                Buffer.BlockCopy(bytee, 0, ret, index, bytee.Length);
                index += bytee.Length;
            }

            return ret;
        }

        protected static byte[] Concat(IEnumerable<byte[]> bytes)
        {
            return Concat(bytes.ToArray());
        }

        protected static byte[] Concat(params byte[][] bytes)
        {
            var size = bytes.Aggregate(0, (intt, bytee) => intt + bytee.Length);
            var ret = new byte[size];
            var index = 0;
            foreach (var bytee in bytes)
            {
                Buffer.BlockCopy(bytee, 0, ret, index, bytee.Length);
                index += bytee.Length;
            }

            return ret;

        }
    }
}
