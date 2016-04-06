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
    enum ObjectType
    {
        INT = 0,
        STRING = 1,
        FLOAT = 2,
        VECTOR3 = 3,
        POINT = 4,
        BYTE = 5,
        GUID = 6,
        MATRIX = 7,
        ENTITY = 8,
        JSON = 9,
        FLOATARRAY = 10,
        BOOL = 11,
        LIST = 12,
        DICTIONARY = 13,
        NETID = 14,
    }
    [ProtoContract]
    public class NetMessage : NetworkSerializable
    {
        [ProtoMember(1)]
        public MemoryStream stream;
        public static EntityCollection ECollection;

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
            Serializer.Serialize<T>(stream, obj);
        }
        public void WriteType(object obj)
        {
            Serializer.NonGeneric.Serialize(stream, obj);
        }
        public T Read<T>()
        {
            return Serializer.Deserialize<T>(stream);
        }
        public object ReadType(Type type)
        {
            return Serializer.NonGeneric.Deserialize(type, stream);
        }

        
        public string ReadString()
        {
            string output = "";
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            int length = BitConverter.ToInt32(buffer, 0);
            for (int i = 0; i < length; i++)
            {
                buffer = new byte[2];
                stream.Read(buffer, 0, 2);
                if (buffer[0] == 0 && buffer[1] == 0) { break; }
                output += BitConverter.ToChar(buffer, 0);
            }
            return output;
        }

        public Vector3 ReadVector()
        {
            return Read<Vector3>();
        }
        
        public Point ReadPoint(Point offset = default(Point))
        {
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            int tx = offset.X + BitConverter.ToInt32(buffer, 0);
            stream.Read(buffer, 0, 4);
            int ty = offset.Y + BitConverter.ToInt32(buffer, 0);
            return new Point(tx, ty);
        }

        public float ReadFloat()
        {
            return BitConverter.ToSingle(ReadBytes(4), 0);
        }
        
        public float[,] Read2DFloatArray()
        {
            float[,] output;
            int xdim = ReadInt();
            int ydim = ReadInt();
            output = new float[xdim, ydim];
            for (int x = 0; x < xdim; x++)
            {
                for (int y = 0; y < ydim; y++)
                {
                    output[x, y] = ReadFloat();
                }
            }
            return output;
        }
        
        public int ReadInt()
        {
            return BitConverter.ToInt32(ReadBytes(4), 0);
        }
        public long ReadLong()
        {
            return BitConverter.ToInt64(ReadBytes(8), 0);
        }
        
        public byte ReadByte()
        {
            if (stream.Position == stream.Length) throw new EndOfStreamException();
            return (byte)stream.ReadByte();
        }
        
        public bool ReadBool()
        {
            return ReadByte() == 1;
        }

        public JToken ReadJSON()
        {
            string json = ReadString();
            return JToken.Parse(json);
        }

        ObjectType GetPrimitiveType(Type t)
        {
            if (t == typeof(int))
                return ObjectType.INT;
            else if (t == typeof(float))
                return ObjectType.FLOAT;
            else if (t == typeof(string))
                return ObjectType.STRING;
            else if (t == typeof(Guid))
                return ObjectType.GUID;
            else if (t == typeof(Vector3))
                return ObjectType.VECTOR3;
            else if (t == typeof(byte))
                return ObjectType.BYTE;
            else if (t == typeof(Matrix))
                return ObjectType.MATRIX;
            else if (t.IsSubclassOf(typeof(Entity)))
                return ObjectType.ENTITY;
            else if (t.IsSubclassOf(typeof(JToken)))
                return ObjectType.JSON;
            else if (t == typeof(float[,]))
                return ObjectType.FLOATARRAY;
            else if (t == typeof(bool))
                return ObjectType.BOOL;
            else if (t == typeof(NetID))
                return ObjectType.NETID;
            else if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                return ObjectType.DICTIONARY;
            throw new SerializationException("Uknown primitive type");
        }
        Type GetPrimitiveType(ObjectType t)
        {
            switch (t)
            {
                case ObjectType.INT:
                    return typeof(int);
                case ObjectType.STRING:
                    return typeof(string);
                case ObjectType.FLOAT:
                    return typeof(float);
                case ObjectType.VECTOR3:
                    return typeof(Vector3);
                case ObjectType.POINT:
                    return typeof(Point);
                case ObjectType.BYTE:
                    return typeof(byte);
                case ObjectType.GUID:
                    return typeof(Guid);
                case ObjectType.MATRIX:
                    return typeof(Matrix);
                case ObjectType.ENTITY:
                    return typeof(Entity);
                case ObjectType.JSON:
                    return typeof(JToken);
                case ObjectType.FLOATARRAY:
                    return typeof(float[,]);
                case ObjectType.BOOL:
                    return typeof(bool);
                case ObjectType.LIST:
                    return typeof(List<>);
                case ObjectType.DICTIONARY:
                    return typeof(Dictionary<,>);
                case ObjectType.NETID:
                    return typeof(NetID);
                default:
                    throw new SerializationException("Uknown primitive type");
            }
        }

        // List
        public List<T> ReadList<T>() where T : NetworkSerializable
        {
            try
            {
                if (!ReadBool()) return null;
                List<T> output = new List<T>();
                int count = ReadInt();
                Type t = typeof(T);
                ConstructorInfo cinfo = t.GetConstructor(new Type[] { typeof(NetMessage) });
                if (cinfo == null) { return output; }
                for (int i = 0; i < count; i++)
                {
                    T toAdd = (T)cinfo.Invoke(new object[] { this });
                    output.Add(toAdd);
                }
                return output;
            }
            catch (Exception e)
            {
                throw new SerializationException(e);
            }
        }
        public void Write<T>(List<T> input) where T : NetworkSerializable
        {
            Write(input != null);
            if(input != null)
            {
                Write(input.Count());
                foreach (NetworkSerializable item in input)
                {
                    item.WriteTo(this);
                }
            }
        }

        public NetworkSerializable Copy()
        {
            NetMessage temp = new NetMessage(stream.ToArray());
            temp.stream.Position = stream.Position;
            return temp;
        }
    }
}
