using Augury.Base;
using System;
using System.IO;

namespace Augury
{
    public class SpellCheckSerializer : Serializer<SpellCheck>
    {
        public override SpellCheck Deserialize(Stream stream)
        {
            var bytes = new byte[4];
            stream.Read(bytes, 0, 4);
            var terminalCount = BitConverter.ToInt32(bytes, 0);

            stream.Read(bytes, 0, 4);
            var len = BitConverter.ToInt32(bytes, 0);
            bytes = new byte[len];
            stream.Read(bytes, 0, len);
            var characters = DeserializeArrayChar(bytes, 0, bytes.Length);

            bytes = new byte[4];
            stream.Read(bytes, 0, 4);
            var rootNodeIndex = BitConverter.ToInt32(bytes, 0);
            
            stream.Read(bytes, 0, 4);
            len = BitConverter.ToInt32(bytes, 0);
            bytes = new byte[len];
            stream.Read(bytes, 0, len);
            var firstChildIndex = DeserializeArrayInt(bytes, 0, len);

            stream.Read(bytes, 0, 4);
            len = BitConverter.ToInt32(bytes, 0);
            bytes = new byte[len];
            stream.Read(bytes, 0, len);
            var edges = DeserializeArrayInt(bytes, 0, len);

            stream.Read(bytes, 0, 4);
            len = BitConverter.ToInt32(bytes, 0);
            bytes = new byte[len];
            stream.Read(bytes, 0, len);
            var characterEdges = DeserializeArrayUShort(bytes, 0, len);

            var dawg = new SpellCheck(terminalCount, characters, rootNodeIndex, firstChildIndex, edges, characterEdges);
            return dawg;
        }

        public override void Serialize(Stream stream, SpellCheck data)
        {
            var bytes = BitConverter.GetBytes(data.TerminalCount);
            stream.Write(bytes, 0, bytes.Length);

            bytes = Serialize(data.Characters);
            stream.Write(bytes, 0, bytes.Length);

            bytes = BitConverter.GetBytes(data.RootNodeIndex);
            stream.Write(bytes, 0, bytes.Length);

            bytes = Encapsulate(Serialize(data.FirstChildIndex));
            stream.Write(bytes, 0, bytes.Length);

            bytes = Encapsulate(Serialize(data.Edges));
            stream.Write(bytes, 0, bytes.Length);

            bytes = Encapsulate(Serialize(data.EdgeCharacter));
            stream.Write(bytes, 0, bytes.Length);

        }
    }
}
