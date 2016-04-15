using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProtoBuf;
using Spectrum.Framework.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Spectrum.Framework.Network
{
    public class SerializationException : ApplicationException
    {
        public SerializationException(string text) : base(text) { }
        public SerializationException(Exception e) : base(e.Message) { }

    }
    [ProtoContract]
    public class NetMessage
    {
        [ProtoMember(1)]
        public MemoryStream stream;

        public NetMessage(byte[] bytes)
        {
            stream = new MemoryStream(bytes);
            stream.Position = 0;
        }
        public NetMessage()
        {
            stream = new MemoryStream();
        }

        public void WriteTo(Stream stream)
        {
            byte[] buffer = this.stream.ToArray();
            stream.Write(BitConverter.GetBytes(buffer.Length), 0, 4);
            stream.Write(buffer, 0, buffer.Length);
        }
        public NetMessage(Stream stream)
        {
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            int size = BitConverter.ToInt32(buffer, 0);
            buffer = new byte[size];
            int totalRead = 0;
            while (totalRead < size)
            {
                int ret = stream.Read(buffer, totalRead, size - totalRead);
                if (ret < 0)
                {

                }
                else
                    totalRead += ret;
            }
            this.stream = new MemoryStream(buffer);
            this.stream.Position = 0;
        }

        public NetMessage(NetMessage message) : this(message.stream) { }

        public void Write(byte[] array, int count)
        {
            stream.Write(array, 0, count);
        }
        public byte[] ReadBytes(int count)
        {
            byte[] output = new byte[count];
            int readBytes = stream.Read(output, 0, count);
            if (readBytes < count) { throw new EndOfStreamException(); }
            return output;
        }

        public void WriteTo(NetMessage message)
        {
            message.Write(this);
        }

        public void Write<T>(T obj)
        {
            Serializer.SerializeWithLengthPrefix<T>(stream, obj, PrefixStyle.Fixed32);
        }
        public void WriteType(object obj)
        {
            Serializer.NonGeneric.SerializeWithLengthPrefix(stream, obj, PrefixStyle.Fixed32, 0);
        }
        public T Read<T>()
        {
            return Serializer.DeserializeWithLengthPrefix<T>(stream, PrefixStyle.Fixed32);
        }
        public object Read(Type type)
        {
            object output;
            bool success = Serializer.NonGeneric.TryDeserializeWithLengthPrefix(stream, PrefixStyle.Fixed32, typeNumber => type, out output);
            return success ? output : null;
        }
    }
}
