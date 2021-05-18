using ProtoBuf;
using Replicate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Network.Surrogates
{
    [ProtoContract]
    public class FloatArraySurrogate
    {
        [Replicate]
        int width;
        [Replicate]
        int height;
        [Replicate]
        float[] buffer;
        public FloatArraySurrogate(int width, int height)
        {
            this.width = width;
            this.height = height;
            buffer = new float[width * height];
        }
        public float this[int i1, int i2]
        {
            get { return buffer[i1 + width * i2]; }
            set { buffer[i1 + width * i2] = value; }
        }
        public static implicit operator FloatArraySurrogate(float[,] array)
        {
            if (array == null) return null;
            float[] buffer = new float[array.GetLength(0) * array.GetLength(1)];
            for (int i = 0; i < array.GetLength(1); i++)
            {
                for (int j = 0; j < array.GetLength(0); j++)
                {
                    buffer[j + i * array.GetLength(0)] = array[j, i];
                }
            }
            return new FloatArraySurrogate(array.GetLength(0), array.GetLength(1)) { buffer = buffer };
        }
        public static implicit operator float[,] (FloatArraySurrogate stream)
        {
            if (stream == null) return null;
            float[,] output = new float[stream.width, stream.height];
            for (int i = 0; i < stream.height; i++)
            {
                for (int j = 0; j < stream.width; j++)
                {
                    output[j, i] = stream.buffer[j + i * stream.width];
                }
            }
            return output;
        }
    }
}
