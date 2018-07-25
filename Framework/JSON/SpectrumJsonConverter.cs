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
    public abstract class SpectrumJsonConverter
    {
        private static Dictionary<string, Type> typeLookup = new Dictionary<string, Type>()
        {
            { "matrix", typeof(Matrix) },
            { "vector", typeof(Vector3) },
            { "vector2", typeof(Vector2) },
            { "vector3", typeof(Vector3) },
            { "vector4", typeof(Vector4) },
        };
        private static HashSet<Type> validTypes = new HashSet<Type>(typeLookup.Values);
        public abstract object Read(JToken token, Type targetType);
        public abstract JToken Write(object obj);
        public static bool IsNumeric(JToken token)
        {
            return token.Type == JTokenType.Float || token.Type == JTokenType.Integer;
        }

        public static object ParseObject(JObject obj, Type targetType)
        {
            string typeName = (string)obj["type"];
            Type type = null;
            if (typeName != null && typeLookup.ContainsKey(typeName))
                type = typeLookup[typeName];
            else if (targetType != null && validTypes.Contains(targetType))
                type = targetType;
            else
                throw new JsonSerializationException(string.Format("Unable to parse an object field"));
            return null;
        }
        
    }
}
