using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectrum.Framework.JSON;
using Spectrum.Framework.Physics.Collision.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Spectrum.Framework
{
    public class JConvert
    {
        public static Dictionary<Type, SpectrumJsonConverter> Converters = new Dictionary<Type, SpectrumJsonConverter>();
        static JConvert()
        {
            var vectorConverter = new JVectorConverter();
            Converters[typeof(Vector2)] = Converters[typeof(Vector3)] = Converters[typeof(Vector4)] = vectorConverter;
            Converters[typeof(Matrix)] = new JMatrixConverter();
            Converters[typeof(Shape)] = Converters[typeof(BoxShape)] = new JShapeParser();
        }
        public static object Read(JToken token, Type targetType)
        {
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                targetType = targetType.GetGenericArguments()[0];
            if (Converters.TryGetValue(targetType, out var converter))
                return converter.Read(token, targetType);
            return null;
        }
        public static T Read<T>(JToken token)
        {
            return (T)(Read(token, typeof(T)) ?? default(T));
        }
        public static T Read<T>(string json)
        {
            using (JsonTextReader reader = new JsonTextReader(new StringReader(json)))
                return (T)(Read(JToken.Load(reader), typeof(T)) ?? default(T));
        }
        public static string Write(object obj)
        {
            var type = obj.GetType();
            if (Converters.TryGetValue(type, out var converter))
                return converter.Write(obj)?.ToString(Formatting.None);
            var sw = new StringWriter();
            new JsonSerializer().Serialize(sw, obj);
            return sw.ToString();
        }
    }
}
