using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectrum.Framework.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
    }
    public class NetMessage : ISerializable
    {
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

        // Sub Message
        public void Write(NetMessage message)
        {
            if (message == null) { Write((int)0); return; }
            byte[] bytes = message.stream.ToArray();
            Write(bytes.Length);
            Write(bytes, bytes.Length);
        }
        public NetMessage ReadMessage()
        {
            int size = ReadInt();
            if (size == 0) { return null; }
            byte[] bytes = ReadBytes(size);
            return new NetMessage(bytes);
        }

        //Net ID
        public void Write(NetID netid)
        {
            if (netid.Guid != null)
            {
                Write(true);
                Write((Guid)netid.Guid);
            }
            else
            {
                Write(false);
                Write((long)netid.SteamID.Value);
            }

        }
        public NetID ReadNetID()
        {
            bool usesGuid = ReadBool();
            if(usesGuid)
                return new NetID(ReadGuid());
            else
                return new NetID((ulong)ReadLong());
        }

        // Guid
        public void Write(Guid guid)
        {
            stream.Write(guid.ToByteArray(), 0, 16);
        }
        public Guid ReadGuid()
        {
            return new Guid(ReadBytes(16));
        }

        // String
        public void Write(string str)
        {
            byte[] buffer = BitConverter.GetBytes(str.Length);
            stream.Write(buffer, 0, 4);
            for (int i = 0; i < str.Length; i++)
            {
                buffer = BitConverter.GetBytes(str[i]);
                stream.Write(buffer, 0, 2);
            }
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

        // Vector3
        public void Write(Vector3 input)
        {
            Byte[] bytes = BitConverter.GetBytes(input.X).ToArray();
            stream.Write(bytes, 0, bytes.Count());
            bytes = BitConverter.GetBytes(input.Y).ToArray();
            stream.Write(bytes, 0, bytes.Count());
            bytes = BitConverter.GetBytes(input.Z).ToArray();
            stream.Write(bytes, 0, bytes.Count());
        }
        public Vector3 ReadVector()
        {
            Vector3 output = new Vector3();
            byte[] bytes = new byte[4];
            stream.Read(bytes, 0, 4);
            output.X = BitConverter.ToSingle(bytes, 0);
            stream.Read(bytes, 0, 4);
            output.Y = BitConverter.ToSingle(bytes, 0);
            stream.Read(bytes, 0, 4);
            output.Z = BitConverter.ToSingle(bytes, 0);
            return output;
        }

        // Point
        public void Write(Point point)
        {
            stream.Write(BitConverter.GetBytes(point.X), 0, 4);
            stream.Write(BitConverter.GetBytes(point.Y), 0, 4);
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

        // Float
        public void Write(float fp)
        {
            stream.Write(BitConverter.GetBytes(fp), 0, 4);
        }
        public float ReadFloat()
        {
            return BitConverter.ToSingle(ReadBytes(4), 0);
        }

        //Matrix
        public void Write(Matrix matrix)
        {
            WritePrimitiveArray(Matrix.ToFloatArray(matrix).ToList().ConvertAll(x => (object)x).ToArray());
        }
        public Matrix ReadMatrix()
        {
            float[] a = ReadPrimitiveArray().ToList().ConvertAll(x => (float)x).ToArray();
            return new Matrix(
                a[0], a[1], a[2], a[3],
                a[4], a[5], a[6], a[7],
                a[8], a[9], a[10], a[11],
                a[12], a[13], a[14], a[15]
                );
        }

        // Float[,]
        public void Write(float[,] fa)
        {
            Write(fa.GetLength(0));
            Write(fa.GetLength(1));
            for (int x = 0; x < fa.GetLength(0); x++)
            {
                for (int y = 0; y < fa.GetLength(1); y++)
                {
                    Write(fa[x, y]);
                }
            }
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

        // Integer
        public void Write(int integer)
        {
            stream.Write(BitConverter.GetBytes(integer), 0, 4);
        }
        public int ReadInt()
        {
            return BitConverter.ToInt32(ReadBytes(4), 0);
        }

        public void Write(long number)
        {
            stream.Write(BitConverter.GetBytes(number), 0, 8);
        }
        public long ReadLong()
        {
            return BitConverter.ToInt64(ReadBytes(8), 0);
        }

        // Byte
        public void Write(byte b)
        {
            stream.WriteByte(b);
        }
        public byte ReadByte()
        {
            if (stream.Position == stream.Length) throw new EndOfStreamException();
            return (byte)stream.ReadByte();
        }

        // Bool
        public void Write(bool b)
        {
            stream.WriteByte((byte)(b ? 1 : 0));
        }
        public bool ReadBool()
        {
            if (stream.Position == stream.Length) throw new EndOfStreamException();
            return stream.ReadByte() == 1;
        }

        public void Write(JToken jobj)
        {
            string json = JsonConvert.SerializeObject(jobj);
            Write(json);
        }
        public JToken ReadJSON()
        {
            string json = ReadString();
            return JToken.Parse(json);
        }

        public void WritePrimitive(object p)
        {
            Type t = p.GetType();
            if (t == typeof(int))
            {
                Write((byte)ObjectType.INT);
                Write((int)p);
            }
            else if (t == typeof(float))
            {
                Write((byte)ObjectType.FLOAT);
                Write((float)p);
            }
            else if (t == typeof(string))
            {
                Write((byte)ObjectType.STRING);
                Write((string)p);
            }
            else if (t == typeof(Guid))
            {
                Write((byte)ObjectType.GUID);
                Write((Guid)p);
            }
            else if (t == typeof(Vector3))
            {
                Write((byte)ObjectType.VECTOR3);
                Write((Vector3)p);
            }
            else if (t == typeof(byte))
            {
                Write((byte)ObjectType.BYTE);
                Write((byte)p);
            }
            else if (t == typeof(Matrix))
            {
                Write((byte)ObjectType.MATRIX);
                Write((Matrix)p);
            }
            else if (t.IsSubclassOf(typeof(Entity)))
            {
                Write((byte)ObjectType.ENTITY);
                Write(((Entity)p).ID);
            }
            else if (t.IsSubclassOf(typeof(JToken)))
            {
                Write((byte)ObjectType.JSON);
                Write(((JToken)p));
            }
            else if (t == typeof(float[,]))
            {
                Write((byte)ObjectType.FLOATARRAY);
                Write((float[,])p);
            }
            else if (t == typeof(bool))
            {
                Write((byte)ObjectType.BOOL);
                Write((bool)p);
            }
            else
            {
                throw new SerializationException("Uknown primitive type");
            }
        }
        public object ReadPrimitive()
        {
            ObjectType type = (ObjectType)ReadByte();
            switch (type)
            {
                case ObjectType.INT:
                    return ReadInt();
                case ObjectType.STRING:
                    return ReadString();
                case ObjectType.FLOAT:
                    return ReadFloat();
                case ObjectType.GUID:
                    return ReadGuid();
                case ObjectType.VECTOR3:
                    return ReadVector();
                case ObjectType.BYTE:
                    return ReadByte();
                case ObjectType.MATRIX:
                    return ReadMatrix();
                case ObjectType.ENTITY:
                    Guid id = ReadGuid();
                    try { return ECollection.Find(id); }
                    catch { throw new SerializationException("Couldn't find entity"); }
                case ObjectType.JSON:
                    return ReadJSON();
                case ObjectType.FLOATARRAY:
                    return Read2DFloatArray();
                case ObjectType.BOOL:
                    return ReadBool();
                default:
                    throw new SerializationException("Uknown primitive type");
            }
        }

        // Constructor Args
        public void WritePrimitiveArray(object[] args)
        {
            Write((byte)args.Length);
            for (int i = 0; i < args.Length; i++)
            {
                WritePrimitive(args[i]);
            }
        }
        public object[] ReadPrimitiveArray()
        {
            try
            {
                int numObjs = ReadByte();
                byte[] buffer = new byte[4];
                object[] output = new object[numObjs];
                for (int i = 0; i < numObjs; i++)
                {
                    output[i] = ReadPrimitive();
                }
                return output;
            }
            catch (Exception e)
            {
                throw new SerializationException(e);
            }
        }

        // List
        public List<T> ReadList<T>() where T : ISerializable
        {
            try
            {
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
        public void Write<T>(List<T> input) where T : ISerializable
        {
            Write(input.Count());
            foreach (ISerializable item in input)
            {
                item.WriteTo(this);
            }
        }

        // Dictionary
        public Dictionary<string, T> ReadDict<T>() where T : ISerializable
        {
            try
            {
                Dictionary<string, T> output = new Dictionary<string, T>();
                int count = ReadInt();
                Type t = typeof(T);
                ConstructorInfo cinfo = t.GetConstructor(new Type[] { typeof(NetMessage) });
                if (cinfo == null) { return output; }
                for (int i = 0; i < count; i++)
                {
                    string key = ReadString();
                    T toAdd = (T)cinfo.Invoke(new object[] { this });
                    output[key] = toAdd;
                }
                return output;
            }
            catch (Exception e)
            {
                throw new SerializationException(e);
            }
        }
        public void Write<T>(Dictionary<string, T> input) where T : ISerializable
        {
            Write(input.Count());
            foreach (KeyValuePair<string, T> item in input)
            {
                Write(item.Key);
                item.Value.WriteTo(this);
            }
        }

        public ISerializable Copy()
        {
            NetMessage temp = new NetMessage();
            this.WriteTo(temp);
            return new NetMessage(temp);
        }
    }
}
