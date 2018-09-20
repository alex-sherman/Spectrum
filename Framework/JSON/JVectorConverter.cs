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
    class JVectorConverter : JsonConverter
    {
        public static bool IsNumeric(JToken token)
        {
            return token.Type == JTokenType.Float || token.Type == JTokenType.Integer;
        }

        public override bool CanConvert(Type objectType)
        {
            objectType = TypeHelper.FixGeneric(objectType);
            return objectType == typeof(Vector2) || objectType == typeof(Vector3) || objectType == typeof(Vector4);
        }

        public override object ReadJson(JsonReader reader, Type targetType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.ReadFrom(reader);
            targetType = TypeHelper.FixGeneric(targetType);
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

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JArray array = null;
            if (value is Vector3 vector3)
                array = new JArray(vector3.X, vector3.Y, vector3.Z);
            if (value is Vector2 vector2)
                array = new JArray(vector2.X, vector2.Y);
            if (value is Vector4 vector4)
                array = new JArray(vector4.X, vector4.Y, vector4.Z, vector4.W);
            var oldF = writer.Formatting;
            writer.Formatting = Formatting.None;
            array.WriteTo(writer);
            writer.Formatting = oldF;
        }
    }
}
