using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Spectrum.Framework.JSON
{
    class JVectorConverter : SpectrumJsonConverter
    {
        public override object Read(JToken token, Type targetType)
        {
            if (token is JArray array)
            {
                if (targetType == typeof(Vector3) && array.Count == 3 && array.All(IsNumeric))
                    return new Vector3((float)array[0], (float)array[1], (float)array[2]);
                if (targetType == typeof(Vector2) && array.Count == 2 && array.All(IsNumeric))
                    return new Vector2((float)array[0], (float)array[1]);
                if (targetType == typeof(Vector4) && array.Count == 4 && array.All(IsNumeric))
                    return new Vector4((float)array[0], (float)array[1], (float)array[2], (float)array[3]);
            }
            return null;
        }

        public override JToken Write(object obj)
        {
            if (obj is Vector3 vector3)
                return new JArray(vector3.X, vector3.Y, vector3.Z);
            if (obj is Vector2 vector2)
                return new JArray(vector2.X, vector2.Y);
            if (obj is Vector4 vector4)
                return new JArray(vector4.X, vector4.Y, vector4.Z, vector4.W);
            return null;
        }
    }
}
