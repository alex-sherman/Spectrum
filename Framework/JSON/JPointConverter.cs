using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.JSON
{
    class JPointConverter : JsonConverter
    {
        public static bool IsNumeric(JToken token)
        {
            return token.Type == JTokenType.Float || token.Type == JTokenType.Integer;
        }

        public override bool CanConvert(Type objectType)
        {
            objectType = TypeHelper.FixGeneric(objectType);
            return objectType == typeof(Point3) || objectType == typeof(Point);
        }

        public override object ReadJson(JsonReader reader, Type targetType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.ReadFrom(reader);
            targetType = TypeHelper.FixGeneric(targetType);
            if (token is JArray array)
            {
                if (targetType == typeof(Point3) && array.Count == 3 && array.All(IsNumeric))
                    return new Point3((int)array[0], (int)array[1], (int)array[2]);
                if (targetType == typeof(Point) && array.Count == 2 && array.All(IsNumeric))
                    return new Point((int)array[0], (int)array[1]);
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JArray array = null;
            if (value is Point3 point3)
                array = new JArray(point3.X, point3.Y, point3.Z);
            if (value is Point point2)
                array = new JArray(point2.X, point2.Y);
            writer.Formatting = Formatting.None;
            array.WriteTo(writer);
            writer.Formatting = Formatting.Indented;
        }
    }
}
